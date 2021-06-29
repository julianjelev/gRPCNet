using gRPCNet.Proto;
using gRPCNet.ServerAPI.BusinessServices;
using gRPCNet.ServerAPI.CommonServices.RestApi;
using gRPCNet.ServerAPI.CommonServices.Utils;
using gRPCNet.ServerAPI.DAL.Repositories;
using gRPCNet.ServerAPI.Models.Domain.Cards;
using gRPCNet.ServerAPI.Models.Domain.Common;
using gRPCNet.ServerAPI.Models.Domain.EGMs;
using gRPCNet.ServerAPI.Models.Domain.Logs;
using gRPCNet.ServerAPI.Models.Domain.Places;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace gRPCNet.ServerAPI.gRPCServices
{
    [Authorize]
    public class CardService : ICardService
    {
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IRestApiClient _restApiClient;

        private readonly string _ownerId;

        public CardService(IHttpContextAccessor contextAccessor, IConfiguration configuration, IServiceProvider serviceProvider, ILogger logger, IRestApiClient restApiClient)
        {
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _restApiClient = restApiClient;
            _ownerId = _configuration.GetSection("AppSettings:OwnerId").Get<string>();
        }

        public async ValueTask<CanPlayResponse> CanPlayAsync(CanPlayRequest value, CallContext context = default)
        {
            Guid guid = Guid.NewGuid();
            _logger.InfoLog($"[CardService.CanPlayAsync][-IN-] [{guid}] TransactionId {value.TransactionId} | ConcentratorId {value.ConcentratorId} | ControllerId {value.ControllerId} | CardType {value.CardType} | CardId {value.CardId} | ShouldPay {value.ShouldPay} | EndpointRssi: {value.EndpointRssi} | ConcentratorRssi {value.ConcentratorRssi}", GetIpAddress());
            // генерира ключ
            var key = $"{value.ControllerId}-{value.CardId}-{value.TransactionId}-{(value.ShouldPay ? 1 : 0)}";
            // извлича тюпъл от кеша за този ключ
            var cachedItem = CanPlayCacheService.Instance.Get(key, true);
            // ако има ключ в тюпъла
            if (!string.IsNullOrWhiteSpace(cachedItem.key))
            {
                // ако има резултат за този ключ в тюпъла
                if (cachedItem.successResponse != null)
                {
                    _logger.InfoLog($"[CardService.CanPlayAsync][-OUT-] [{guid}] TransactionId {value.TransactionId} | ConcentratorId {value.ConcentratorId} | ControllerId {value.ControllerId} | CardType {value.CardType} | CardId {value.CardId} | ShouldPay {value.ShouldPay} | EndpointRssi: {value.EndpointRssi} | ConcentratorRssi {value.ConcentratorRssi} | CACHED Result", GetIpAddress());
                    // връща резултата от тюпъла
                    return cachedItem.successResponse;
                }
            }
            // резервира семафора за текущия тред и продължава, други тредове стигнали до тук чакат ...
            await semaphoreSlim.WaitAsync();
            // проверява за евнтуален тюпъл и резултат за този ключ от други тредове
            cachedItem = CanPlayCacheService.Instance.Get(key);
            if (cachedItem.successResponse != null)
            {
                _logger.InfoLog($"[CardService.CanPlayAsync][-OUT-] [{guid}] TransactionId {value.TransactionId} | ConcentratorId {value.ConcentratorId} | ControllerId {value.ControllerId} | CardType {value.CardType} | CardId {value.CardId} | ShouldPay {value.ShouldPay} | EndpointRssi: {value.EndpointRssi} | ConcentratorRssi {value.ConcentratorRssi} | CACHED Result", GetIpAddress());
                // освобождава семафора за следващи тредове, които чакат
                semaphoreSlim.Release();
                // връща резултата от тюпъла
                return cachedItem.successResponse;
            }
            // резервира ключа в кеша, като създава тюпъл с този ключ, но без резултат
            CanPlayCacheService.Instance.Add(key, (key, null));
            CanPlayResponse response = null;
            try 
            {
                // извлича резултата от апи-то (отнема много време)
                var tupleResult = await CanPlayAsyncInternal(guid, value.ConcentratorId, value.ControllerId, value.CardType, value.CardId, value.ShouldPay, value.TransactionId, value.EndpointRssi, value.ConcentratorRssi);
                if (tupleResult.success)
                    // обновява резултата в тюпъла в кеша за резервирания ключ 
                    CanPlayCacheService.Instance.Add(key, (key, tupleResult.response));
                // връща резултата от апи-то
                response = tupleResult.response;
            }
            finally
            {
                // освобождава семафора за следващи тредове, които чакат
                semaphoreSlim.Release();
            }

            return response ?? new CanPlayResponse 
            {
                TransactionId = value.TransactionId,
                Time = DateTime.UtcNow,
                ResponseCode = 109,
                ConcentratorId = value.ConcentratorId,
                ControllerId = value.ControllerId,
                CardType = value.CardType,
                CardId = value.CardId,
                CardNumber = string.Empty,
                ServiceId = string.Empty,
                ServiceName = string.Empty,
                Permission = false,
                RelayType = string.Empty,
                RelayPulse = 0,
                RelayOnTime = 0,
                RelayOffTime = 0,
                RelayDisplayTime = 0,
                DisplayLine1 = "",
                DisplayLine2 = ""
            };
        }

        public async ValueTask<ServicePriceResponse> ServicePriceAsync(ServicePriceRequest value, CallContext context = default)
        {
            Guid guid = Guid.NewGuid();
            _logger.InfoLog($"[CardService.ServicePriceAsync][-IN-] [{guid}] TransactionId {value.TransactionId} | ConcentratorId {value.ConcentratorId} | ControllerId {value.ControllerId} | CardType {value.CardType} | CardId {value.CardId} | ServiceId {value.ServiceId} | EndpointRssi: {value.EndpointRssi} | ConcentratorRssi {value.ConcentratorRssi}", GetIpAddress());
            // генерира ключ
            var key = $"{value.ControllerId}-{value.CardId}-{value.TransactionId}-{value.ServiceId}";
            // извлича тюпъл от кеша за този ключ
            var cachedItem = ServicePriceCacheService.Instance.Get(key, true);
            // ако има ключ в тюпъла
            if (!string.IsNullOrWhiteSpace(cachedItem.key))
            {
                // ако има резултат за този ключ в тюпъла
                if (cachedItem.successResponse != null)
                {
                    _logger.InfoLog($"[CardService.ServicePriceAsync][-OUT-] [{guid}] TransactionId {value.TransactionId} | ConcentratorId {value.ConcentratorId} | ControllerId {value.ControllerId} | CardType {value.CardType} | CardId {value.CardId} | ServiceId {value.ServiceId} | EndpointRssi: {value.EndpointRssi} | ConcentratorRssi {value.ConcentratorRssi} | CACHED Result", GetIpAddress());
                    // връща резултата от тюпъла
                    return cachedItem.successResponse;
                }
            }
            // резервира семафора за текущия тред и продължава, други тредове стигнали до тук чакат ...
            await semaphoreSlim.WaitAsync();
            // проверява за евнтуален тюпъл и резултат за този ключ от други тредове
            cachedItem = ServicePriceCacheService.Instance.Get(key);
            if (cachedItem.successResponse != null)
            {
                _logger.InfoLog($"[CardService.ServicePriceAsync][-OUT-] [{guid}] TransactionId {value.TransactionId} | ConcentratorId {value.ConcentratorId} | ControllerId {value.ControllerId} | CardType {value.CardType} | CardId {value.CardId} | ServiceId {value.ServiceId} | EndpointRssi: {value.EndpointRssi} | ConcentratorRssi {value.ConcentratorRssi} | CACHED Result", GetIpAddress());
                // освобождава семафора за следващи тредове, които чакат
                semaphoreSlim.Release();
                // връща резултата от тюпъла
                return cachedItem.successResponse;
            }
            // резервира ключа в кеша, като създава тюпъл с този ключ, но без резултат
            ServicePriceCacheService.Instance.Add(key, (key, null));
            ServicePriceResponse response = null;
            try 
            {
                // извлича резултата от апи-то (отнема много време)
                var tupleResult = ServicePriceInternal(guid, value.ConcentratorId, value.ControllerId, value.CardType, value.CardId, value.ServiceId, value.TransactionId, value.EndpointRssi, value.ConcentratorRssi);
                if (tupleResult.success)
                    // обновява резултата в тюпъла в кеша за резервирания ключ 
                    ServicePriceCacheService.Instance.Add(key, (key, tupleResult.response));
                // връща резултата от апи-то
                response = tupleResult.response;
            }
            finally
            {
                // освобождава семафора за следващи тредове, които чакат
                semaphoreSlim.Release();
            }

            return response ?? new ServicePriceResponse 
            {
                TransactionId = value.TransactionId,
                Time = DateTime.UtcNow,
                ResponseCode = 109,
                ConcentratorId = value.ConcentratorId,
                ControllerId = value.ControllerId,
                CardType = value.CardType,
                CardId = value.CardId,
                CardNumber = string.Empty,
                ServiceId = string.Empty,
                ServiceName = string.Empty,
                Price = 0
            };
        }


        #region private

        private string GetIpAddress() => _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

        private async ValueTask<(bool success, CanPlayResponse response)> CanPlayAsyncInternal(
            Guid guid,
            string concentratorId,
            string controllerId,
            int cardType,
            string cardId,
            bool shouldPay,
            int transactionId,
            int endpointRssi,
            int concentratorRssi) 
        {
            Concentrator getConcentrator() 
            {
                var repo = _serviceProvider.GetService<ICardSystemRepository<Concentrator>>();
                return repo.AsNoTracking().FirstOrDefault(x => x.DeviceId == concentratorId);
            };
            ControllerConfigRelay getController()
            {
                var repo = _serviceProvider.GetService<ICardSystemRepository<ControllerConfigRelay>>();
                return repo.AsNoTracking()
                    .Include(x => x.EGM)
                    .ThenInclude(x => x.AllowedCurrencies)
                    .FirstOrDefault(x => x.SerialNumber == controllerId && x.OwnerId == _ownerId);
            };
            Card getCard() 
            {
                Card entity = null;
                ICardSystemRepository<Card> repo = null;
                string cardDBId = CardCacheServiceV2.Instance.GetIdByExternalId(cardId);
                if (!string.IsNullOrWhiteSpace(cardDBId)) 
                {
                    entity = CardCacheServiceV2.Instance.GetCard(_serviceProvider, cardDBId);
                    if (entity != null) 
                        return entity;
                    repo = _serviceProvider.GetService<ICardSystemRepository<Card>>();
                    entity = repo.AsNoTracking()
                             .Include(x => x.CardMode)
                             .Include(x => x.User)
                             .FirstOrDefault(x => x.Id == cardDBId);
                    if (entity != null)
                    {
                        CardCacheServiceV2.Instance.AddCard(_serviceProvider, entity);
                        CardCacheServiceV2.Instance.AddNewMap(entity);
                    }
                    return entity;
                }

                repo = _serviceProvider.GetService<ICardSystemRepository<Card>>();
                entity = repo.AsNoTracking()
                        .Include(x => x.CardMode)
                        .Include(x => x.User)
                        .FirstOrDefault(x => x.ExternalId == cardId);
                if (entity != null)
                {
                    CardCacheServiceV2.Instance.AddCard(_serviceProvider, entity);
                    CardCacheServiceV2.Instance.AddNewMap(entity);
                }
                return entity;
            };
            IEnumerable<CurrencyTemplate> getCurrencyTemplates() 
            {
                var repo = _serviceProvider.GetService<ICardSystemRepository<CurrencyTemplate>>();
                return repo.AsNoTracking()
                    .Where(x => x.OwnerId == _ownerId)
                    .ToList();
            }
            
            var concentrator = getConcentrator();
            if (concentrator == null) 
            {
                var response = new CanPlayResponse
                {
                    TransactionId = transactionId,
                    Time = DateTime.UtcNow,
                    ResponseCode = 101,
                    ConcentratorId = concentratorId,
                    ControllerId = controllerId,
                    CardType = cardType,
                    CardId = cardId,
                    CardNumber = string.Empty,
                    ServiceId = string.Empty,
                    ServiceName = string.Empty,
                    Permission = false,
                    RelayType = string.Empty,
                    RelayPulse = 0,
                    RelayOnTime = 0,
                    RelayOffTime = 0,
                    RelayDisplayTime = 0,
                    DisplayLine1 = "Err 101",
                    DisplayLine2 = "Concentrator N/A"
                };
                _logger.ErrorLog($"[CardService.CanPlayAsyncInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ShouldPay {shouldPay} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
                return (false, response);
            }
            var controller = getController();
            if (controller == null) 
            {
                var response = new CanPlayResponse
                {
                    TransactionId = transactionId,
                    Time = DateTime.UtcNow,
                    ResponseCode = 102,
                    ConcentratorId = concentratorId,
                    ControllerId = controllerId,
                    CardType = cardType,
                    CardId = cardId,
                    CardNumber = string.Empty,
                    ServiceId = string.Empty,
                    ServiceName = string.Empty,
                    Permission = false,
                    RelayType = string.Empty,
                    RelayPulse = 0,
                    RelayOnTime = 0,
                    RelayOffTime = 0,
                    RelayDisplayTime = 0,
                    DisplayLine1 = "Err 102",
                    DisplayLine2 = "Controller N/A"
                };
                _logger.ErrorLog($"[CardService.CanPlayAsyncInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ShouldPay {shouldPay} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
                return (false, response);
            }
            var card = getCard();
            if (card == null) 
            {
                var response = new CanPlayResponse
                {
                    TransactionId = transactionId,
                    Time = DateTime.UtcNow,
                    ResponseCode = 103,
                    ConcentratorId = concentratorId,
                    ControllerId = controllerId,
                    CardType = cardType,
                    CardId = cardId,
                    CardNumber = string.Empty,
                    ServiceId = string.Empty,
                    ServiceName = string.Empty,
                    Permission = false,
                    RelayType = string.Empty,
                    RelayPulse = 0,
                    RelayOnTime = 0,
                    RelayOffTime = 0,
                    RelayDisplayTime = 0,
                    DisplayLine1 = "Err 103",
                    DisplayLine2 = "Card N/A"
                };
                _logger.ErrorLog($"[CardService.CanPlayAsyncInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ShouldPay {shouldPay} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
                return (false, response);
            }
            var currencyTemplates = getCurrencyTemplates();

            IEnumerable<ScheduleDay> getScheduleDays()
            {
                IEnumerable<ScheduleDay> egmPriceModifiers = null;
                var repo = _serviceProvider.GetService<ICardSystemRepository<ScheduleDay>>();
                if (!string.IsNullOrWhiteSpace(controller.EGM.Id))
                    egmPriceModifiers = repo.AsNoTracking()
                        .Include(x => x.Hours)
                        .Where(x => x.EGMId == controller.EGM.Id)
                        .AsEnumerable();
                if (egmPriceModifiers != null && !egmPriceModifiers.Any())
                    egmPriceModifiers = repo
                        .AsNoTracking()
                        .Include(x => x.Hours)
                        .Where(x => x.GameCenterId == controller.EGM.GameCenterId)
                        .AsEnumerable();
                return egmPriceModifiers ?? new List<ScheduleDay>();
            }
            var scheduleDays = getScheduleDays();

            // ако се изиква плащане този метод променя баланса на картата
            var canPlayDto = _serviceProvider.GetService<ICanPlayService>().CanPlayV2(
                _serviceProvider, 
                concentrator,
                controller,
                card,
                scheduleDays.ToList(),
                shouldPay,
                currencyTemplates);

            if (canPlayDto.HasError) 
            {
                var errors = canPlayDto.Message.Split('|');
                var response = new CanPlayResponse
                {
                    TransactionId = transactionId,
                    Time = DateTime.UtcNow,
                    ResponseCode = int.Parse(errors[0]),
                    ConcentratorId = concentratorId,
                    ControllerId = controllerId,
                    CardType = cardType,
                    CardId = cardId,
                    CardNumber = string.Empty,
                    ServiceId = string.Empty,
                    ServiceName = string.Empty,
                    Permission = false,
                    RelayType = string.Empty,
                    RelayPulse = 0,
                    RelayOnTime = 0,
                    RelayOffTime = 0,
                    RelayDisplayTime = 0,
                    DisplayLine1 = $"Err {errors[0]}",
                    DisplayLine2 = errors[1]
                };
                _logger.ErrorLog($"[CardService.CanPlayAsyncInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ShouldPay {shouldPay} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
                return (false, response);
            }

            EGM gameDto = null;
            if (canPlayDto.CanPlay) gameDto = controller.EGM;

            var canPlayResponse = new CanPlayResponse 
            {
                TransactionId = transactionId,
                Time = DateTime.UtcNow,
                ResponseCode = 0,
                ConcentratorId = concentrator.DeviceId,
                ControllerId = controller.SerialNumber,
                CardType = card.Type,
                CardId = card.ExternalId,
                CardNumber = card.Number,
                ServiceId = canPlayDto.GameId,
                ServiceName = canPlayDto.GameName,
                Permission = canPlayDto.CanPlay,
                RelayType = gameDto?.RelayMode,
                RelayPulse = gameDto?.RelayPulse ?? 0,
                RelayOnTime = gameDto?.RelayOnTime ?? 0,
                RelayOffTime = gameDto?.RelayOffTime ?? 0,
                RelayDisplayTime = gameDto?.RelayDisplayTime ?? 0,
                DisplayLine1 = $"{canPlayDto.PricePerGame:F2}".Trim().Trim(',').Trim(),
                DisplayLine2 = $"{canPlayDto.RemainingBalance:F2}"
            };

            // само ако се изисква плащане логира плащането в транзакшън лога и обновява кеша на картата в api.icardmanager.eu
            if (shouldPay) 
            {
                canPlayDto.GameId = gameDto?.Id;
                string relayInfo = $" | Relay ontime: {gameDto?.RelayOnTime}, Relay offtime: {gameDto?.RelayOffTime}, Relay pulse: {gameDto?.RelayPulse}, Relay display time: {gameDto?.RelayDisplayTime}, Relay mode: {gameDto?.RelayMode}, Game Serial Number: '{gameDto?.SerialNumber}'";
                CreateCanPlayLog(
                    concentrator.DeviceId,
                    controller.SerialNumber,
                    $"{canPlayDto.GameCenterName}{relayInfo}",
                    canPlayDto.GameId,
                    canPlayDto.GameName,
                    canPlayDto.CurrencyInfo,
                    canPlayDto.CardId,
                    canPlayDto.CardType,
                    canPlayDto.PricePerGame,
                    canPlayDto.PaidFromBonusBalance,
                    canPlayDto.CanPlay,
                    gameDto?.GameCenterId,
                    gameDto != null ? gameDto.OwnerId : "system"
                    );
                try 
                {
                    var apiResponse = await _restApiClient.GetAsync<string>(_configuration.GetSection("AppSettings:ApiServiceURL").Get<string>(), $"api/{_ownerId}/card/{card.Id}/reloadcache");
                    if (apiResponse.hasError)
                        _logger.ErrorLog($"[CardService.CanPlayAsyncInternal][-API-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ShouldPay {shouldPay} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {apiResponse.errorMessage}", GetIpAddress());
                }
                catch (Exception ex) 
                {
                    _logger.LogException($"CardService.CanPlayAsyncInternal throws: {ex}");
                }
            }

            _logger.InfoLog($"[CardService.CanPlayAsyncInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ShouldPay {shouldPay} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | DB Result {JsonSerializer.Serialize(canPlayResponse, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
            return (true, canPlayResponse);
        }

        private (bool success, ServicePriceResponse response) ServicePriceInternal(
            Guid guid,
            string concentratorId,
            string controllerId,
            int cardType,
            string cardId,
            string serviceId,
            int transactionId,
            int endpointRssi,
            int concentratorRssi) 
        {
            Concentrator getConcentrator()
            {
                var repo = _serviceProvider.GetService<ICardSystemRepository<Concentrator>>();
                return repo.AsNoTracking().FirstOrDefault(x => x.DeviceId == concentratorId);
            };
            ControllerConfigRelay getController()
            {
                var repo = _serviceProvider.GetService<ICardSystemRepository<ControllerConfigRelay>>();
                return repo.AsNoTracking()
                    .Include(x => x.EGM)
                    .ThenInclude(x => x.AllowedCurrencies)
                    .FirstOrDefault(x => x.SerialNumber == controllerId && x.OwnerId == _ownerId);
            };
            Card getCard()
            {
                Card entity = null;
                ICardSystemRepository<Card> repo = null;
                string cardDBId = CardCacheServiceV2.Instance.GetIdByExternalId(cardId);
                if (!string.IsNullOrWhiteSpace(cardDBId))
                {
                    entity = CardCacheServiceV2.Instance.GetCard(_serviceProvider, cardDBId);
                    if (entity != null)
                        return entity;
                    repo = _serviceProvider.GetService<ICardSystemRepository<Card>>();
                    entity = repo.AsNoTracking()
                             .Include(x => x.CardMode)
                             .Include(x => x.User)
                             .FirstOrDefault(x => x.Id == cardDBId);
                    if (entity != null)
                    {
                        CardCacheServiceV2.Instance.AddCard(_serviceProvider, entity);
                        CardCacheServiceV2.Instance.AddNewMap(entity);
                    }
                    return entity;
                }

                repo = _serviceProvider.GetService<ICardSystemRepository<Card>>();
                entity = repo.AsNoTracking()
                        .Include(x => x.CardMode)
                        .Include(x => x.User)
                        .FirstOrDefault(x => x.ExternalId == cardId);
                if (entity != null)
                {
                    CardCacheServiceV2.Instance.AddCard(_serviceProvider, entity);
                    CardCacheServiceV2.Instance.AddNewMap(entity);
                }
                return entity;
            };
            EGM getEGM() 
            {
                var repo = _serviceProvider.GetService<ICardSystemRepository<EGM>>();
                return repo.AsNoTracking()
                                .Include(x => x.AllowedCurrencies)
                                .Include(x => x.ConfigRelays)
                                .Include(x => x.GameCategory)
                                .Include(x => x.GameManageControllers)
                                .Include(x => x.Images)
                                .Include(x => x.LightingEvent)
                                .Include(x => x.LightingSignals)
                                .Include(x => x.ScheduleDays)
                                .FirstOrDefault(x => x.Id == serviceId);
            }

            var concentrator = getConcentrator();
            if (concentrator == null)
            {
                var response = new ServicePriceResponse
                {
                    TransactionId = transactionId,
                    Time = DateTime.UtcNow,
                    ResponseCode = 101,
                    ConcentratorId = concentratorId,
                    ControllerId = controllerId,
                    CardType = cardType,
                    CardId = cardId,
                    CardNumber = string.Empty,
                    ServiceId = string.Empty,
                    ServiceName = string.Empty,
                    Price = 0
                };
                _logger.ErrorLog($"[CardService.ServicePriceInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ServiceId {serviceId} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
                return (false, response);
            }
            var controller = getController();
            if (controller == null)
            {
                var response = new ServicePriceResponse
                {
                    TransactionId = transactionId,
                    Time = DateTime.UtcNow,
                    ResponseCode = 102,
                    ConcentratorId = concentratorId,
                    ControllerId = controllerId,
                    CardType = cardType,
                    CardId = cardId,
                    CardNumber = string.Empty,
                    ServiceId = string.Empty,
                    ServiceName = string.Empty,
                    Price = 0
                };
                _logger.ErrorLog($"[CardService.ServicePriceInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ServiceId {serviceId} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
                return (false, response);
            }
            var card = getCard();
            if (card == null)
            {
                var response = new ServicePriceResponse
                {
                    TransactionId = transactionId,
                    Time = DateTime.UtcNow,
                    ResponseCode = 103,
                    ConcentratorId = concentratorId,
                    ControllerId = controllerId,
                    CardType = cardType,
                    CardId = cardId,
                    CardNumber = string.Empty,
                    ServiceId = string.Empty,
                    ServiceName = string.Empty,
                    Price = 0
                };
                _logger.ErrorLog($"[CardService.ServicePriceInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ServiceId {serviceId} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
                return (false, response);
            }
            var egm = getEGM();
            if (egm == null)
            {
                var response = new ServicePriceResponse
                {
                    TransactionId = transactionId,
                    Time = DateTime.UtcNow,
                    ResponseCode = 132,
                    ConcentratorId = concentratorId,
                    ControllerId = controllerId,
                    CardType = cardType,
                    CardId = cardId,
                    CardNumber = string.Empty,
                    ServiceId = string.Empty,
                    ServiceName = string.Empty,
                    Price = 0
                };
                _logger.ErrorLog($"[CardService.ServicePriceInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ServiceId {serviceId} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
                return (false, response);
            }

            IEnumerable<ScheduleDay> getScheduleDays()
            {
                IEnumerable<ScheduleDay> egmPriceModifiers = null;
                var repo = _serviceProvider.GetService<ICardSystemRepository<ScheduleDay>>();
                if (!string.IsNullOrWhiteSpace(controller.EGM.Id))
                    egmPriceModifiers = repo.AsNoTracking()
                        .Include(x => x.Hours)
                        .Where(x => x.EGMId == controller.EGM.Id)
                        .AsEnumerable();
                if (egmPriceModifiers != null && !egmPriceModifiers.Any())
                    egmPriceModifiers = repo
                        .AsNoTracking()
                        .Include(x => x.Hours)
                        .Where(x => x.GameCenterId == controller.EGM.GameCenterId)
                        .AsEnumerable();
                return egmPriceModifiers ?? new List<ScheduleDay>();
            }
            var scheduleDays = getScheduleDays();

            decimal price = _serviceProvider.GetService<ICanPlayService>().GetPricePerGame(
                _serviceProvider,
                scheduleDays.ToList(),
                egm);

            ServicePriceResponse servicePriceResponse = new ServicePriceResponse
            {
                TransactionId = transactionId,
                Time = DateTime.UtcNow,
                ResponseCode = 0,
                ConcentratorId = concentrator.DeviceId,
                ControllerId = controller.SerialNumber,
                CardType = card.Type,
                CardId = card.ExternalId,
                CardNumber = card.Number,
                ServiceId = egm.Id,
                ServiceName = egm.MachineName,
                Price = (double)price
            };

            _logger.InfoLog($"[CardService.ServicePriceInternal][-OUT-] [{guid}] TransactionId {transactionId} | ConcentratorId {concentratorId} | ControllerId {controllerId} | CardType {cardType} | CardId {cardId} | ServiceId {serviceId} | EndpointRssi: {endpointRssi} | ConcentratorRssi {concentratorRssi} | DB Result {JsonSerializer.Serialize(servicePriceResponse, new JsonSerializerOptions { WriteIndented = false })}", GetIpAddress());
            return (true, servicePriceResponse);
        }

        private void CreateCanPlayLog(
            string concentratorId,
            string gameControllerId,
            string gameCenterName,
            string gameId,
            string gameName,
            string currencyInfo,
            string cardId,
            string cardType,
            decimal pricePerGame,
            decimal paidFromBonusBalance,
            bool canPlay,
            string placeId,
            string ownerId
            ) 
        {
            var additionalInfo = new StringBuilder();
            additionalInfo.Append($"Concentrator ID: {concentratorId} | Game controller ID: {gameControllerId}");
            if (!string.IsNullOrWhiteSpace(gameCenterName))
            {
                additionalInfo.Append($"; Game Center: {gameCenterName}");
            }

            if (!string.IsNullOrWhiteSpace(gameName))
            {
                additionalInfo.Append($"; Game: {gameName}");
            }

            if (!string.IsNullOrWhiteSpace(currencyInfo))
            {
                additionalInfo.Append($"; Currency: {currencyInfo}");
            }

            TransactionLog log = new TransactionLog()
            {
                CardId = cardId,
                CardType = cardType,
                Service = "play game",
                Amount = Math.Abs(pricePerGame) * -1,
                SenderIP = GetIpAddress(),
                IsSuccess = canPlay,
                AdditionalInfo = additionalInfo.ToString(),
                AmountAsBonus = paidFromBonusBalance,
                PlaceId = placeId,
                OwnerId = ownerId,
                GameId = gameId
            };
            try 
            {
                var repo = _serviceProvider.GetService<ITransactionLogRepository<TransactionLog>>();
                repo.Add(log);
            }
            catch (Exception ex) 
            {
                _logger.LogException($"CardService.CreateCanPlayLog throws: {ex}");
            }
        }

        #endregion
    }
}
