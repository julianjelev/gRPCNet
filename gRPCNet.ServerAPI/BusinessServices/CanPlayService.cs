using gRPCNet.ServerAPI.CommonServices.Utils;
using gRPCNet.ServerAPI.Constants.Cards;
using gRPCNet.ServerAPI.DAL.Repositories;
using gRPCNet.ServerAPI.Models.Domain.Cards;
using gRPCNet.ServerAPI.Models.Domain.Common;
using gRPCNet.ServerAPI.Models.Domain.EGMs;
using gRPCNet.ServerAPI.Models.Domain.Places;
using gRPCNet.ServerAPI.Models.Dto.Common;
using gRPCNet.ServerAPI.Models.Dto.EGM;
using gRPCNet.ServerAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gRPCNet.ServerAPI.BusinessServices
{
    public class CanPlayService : ICanPlayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public CanPlayService(IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = new Logger();
        }

        public CanPlayDto CanPlayV2(
            IServiceProvider serviceProvider,
            Concentrator concentrator,
            ControllerConfigRelay gameController,
            Card card,
            IList<ScheduleDay> scheduleDays,
            bool shouldPay,
            IEnumerable<CurrencyTemplate> currencyTemplates)
        {
            var result = new CanPlayDto();

            if (concentrator == null)
            {
                result.HasError = true;
                result.Message = "111|Invalid requirements! Concentrator is null";
                return result;
            }
            if (gameController == null)
            {
                result.HasError = true;
                result.Message = "112|Invalid requirements! Game controller is null";
                return result;
            }
            if (gameController.EGM == null || concentrator.GameCenterId != gameController.EGM.GameCenterId)
            {
                result.HasError = true;
                result.Message = "114|Invalid requirements! gameController.EGM == null or concentrator.GameCenterId != gameController.EGM.GameCenterId";
                return result;
            }
            if (card == null)
            {
                result.HasError = true;
                result.Message = "113|Invalid requirements! Card is null";
                return result;
            }
            if (!IsCardValidV2(card))
            {
                result.HasError = true;
                result.Message = "104|Card is not valid!";
                return result;
            }
            if (gameController.MinTransactionTime <= 0)
            {
                result.HasError = true;
                result.Message = "105|Controller's MinTransactionTime <= 0";
                return result;
            }
            if ((DateTime.UtcNow - card.LastTransactionTime).TotalSeconds < gameController.MinTransactionTime)
            {
                result.HasError = true;
                result.Message = "106|Transaction timespan is too low!";
                return result;
            }

            decimal pricePerGame = GetPricePerGame(serviceProvider, scheduleDays, gameController.EGM);
            int discount = 0;
            if (card.CardMode != null)
            {
                discount = card.CardMode.DiscountPercent;
            }
            if (discount != 0)
            {
                pricePerGame = CalculateByDiscount(pricePerGame, discount);
            }
            if (card.FreePeriod) 
            {
                #region Free-Period
                if (card.FreePeriodStart < DateTime.UtcNow && DateTime.UtcNow < card.FreePeriodEnd) //is Active Free Period
                {
                    result.CanPlay = true;
                    result.GameCenterName = gameController.EGM.GameCenter?.Name;
                    result.GameName = gameController.EGM.MachineName;
                    result.CurrencyInfo = "FP";
                    result.RemainingBalance = card.GetBalanceFromSomeCurrency();
                    if (shouldPay)
                    {
                        card.LastTransactionTime = DateTime.UtcNow;
                        var updateResult = UpdateCurrency(serviceProvider, card);
                        if (!updateResult.IsSuccess)
                        {
                            result.CanPlay = false;
                            result.HasError = true;
                            result.Message = "117|Cannot update Card with ActiveFreePeriod";
                            return result;
                        }
                    }
                    return result;
                }
                else 
                {
                    card.FreePeriod = false;
                    card.FreePeriodStart = null;
                    card.FreePeriodEnd = null;
                    var updateResult = UpdateCurrency(serviceProvider, card);
                    if (!updateResult.IsSuccess)
                    {
                        result.CanPlay = false;
                        result.HasError = true;
                        result.Message = "107|Cannot update Card";
                        return result;
                    }
                }
                #endregion
            }

            bool tempCanPlay = false;
            string tempCurrencyName = null;
            AllowedCurrency[] egmCurrencies = gameController.EGM.AllowedCurrencies.OrderByDescending(x => x.Order).ToArray();
            string currencyType = string.Empty;
            foreach (var egmC in egmCurrencies) 
            {
                if (egmC.Type == CCurrency.Credit)
                {
                    currencyType = CCurrency.Credit;
                    tempCanPlay = card.CurrencyPrimaryBalance + card.CurrencyPrimaryBalanceBonus >= pricePerGame;
                }
                else if (egmC.Type == CCurrency.Courtesy)
                {
                    tempCanPlay = card.CurrencyCreditBalance + card.CurrencyCreditBalanceBonus >= pricePerGame;
                    currencyType = CCurrency.Courtesy;
                }
                else if (egmC.Type == CCurrency.Bonus)
                {
                    tempCanPlay = card.CurrencyBonusBalance + card.CurrencyBonusBalanceBonus >= pricePerGame;
                    currencyType = CCurrency.Bonus;
                }
                else if (egmC.Type == CCurrency.TicketBonus)
                {
                    tempCanPlay = card.CurrencyTicketBalance + card.CurrencyTicketBalanceBonus >= pricePerGame;
                    currencyType = CCurrency.TicketBonus;
                }
                else if (egmC.Type == CCurrency.Minutes)
                {
                    tempCanPlay = card.CurrencyTimeBalance + card.CurrencyTimeBalanceBonus >= pricePerGame;
                    currencyType = CCurrency.Minutes;
                }
                else if (egmC.Type == CCurrency.TicketUsedBonus)
                {
                    tempCanPlay = card.Currency1Balance + card.Currency1BalanceBonus >= pricePerGame;
                    currencyType = CCurrency.TicketUsedBonus;
                }

                if (tempCanPlay)
                {
                    tempCurrencyName = egmC.Type;
                    break;
                }
            }
            if (!tempCanPlay)
            {
                result.Message = "108|Balance is to low or game does not support current currency!";
                result.HasError = true;
            }
            if (result.HasError)
            {
                result.CanPlay = false;
                result.PricePerGame = pricePerGame;
                result.GameCenterName = gameController.EGM.GameCenter?.Name;
                result.GameName = gameController.EGM.MachineName;
                if (!string.IsNullOrWhiteSpace(tempCurrencyName))
                {
                    // TODO: get currency code by currency template
                    result.CurrencyInfo = "";// tempCurrency.Code;
                    result.RemainingBalance = GetBalanceByCurrencyType(card, tempCurrencyName);
                }
                return result;
            }

            try 
            {
                if (shouldPay)
                {
                    (decimal paidFromPrimary, decimal paidFromBonus) = PaymentByCurrencyType(card, tempCurrencyName, pricePerGame);
                    result.PaidFromPrimaryBalance = paidFromPrimary;
                    result.PaidFromBonusBalance = paidFromBonus;
                    card.LastTransactionTime = DateTime.UtcNow;
                    if (gameController.EGM.BonusAfterPlay > 0)
                    {
                        card.CurrencyCreditBalanceBonus += gameController.EGM.BonusAfterPlay;
                        result.HasBonusAfterPlay = true;
                        result.BonusAfterPlay = gameController.EGM.BonusAfterPlay;
                    }
                    var updateResult = UpdateCurrency(serviceProvider, card);
                    if (!updateResult.IsSuccess)
                    {
                        result.CanPlay = false;
                        result.HasError = true;
                        result.Message = "127|Cannot update Card with shouldPay = true";
                        return result;
                    }
                }
                result.CanPlay = true;
                result.PricePerGame = pricePerGame;
                result.GameCenterName = gameController.EGM.GameCenter?.Name;
                result.GameId = gameController.EGM.Id;
                result.GameName = gameController.EGM.MachineName;
                CurrencyTemplate currencyTemplate = currencyTemplates.FirstOrDefault(x => x.Type == currencyType);
                if (currencyTemplate != null)
                {
                    result.CurrencyInfo = currencyTemplate.Code;
                }
                result.RemainingBalance = GetBalanceByCurrencyType(card, tempCurrencyName);
                result.CardId = card.Id;
                result.CardNumber = card.Number;
                result.CardType = card.TypeName;
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.LogException($"BusinessServices.CanPlayService.CanPlayV2 throws: {ex}");
                result.HasError = true;
                result.Message = "109|Internal Server Error! Cannot update card state!";
            }

            return result;
        }

        public decimal GetPricePerGame(
            IServiceProvider serviceProvider,
            IList<ScheduleDay> egmPriceModifiers,
            EGM game)
        {
            if (egmPriceModifiers == null || !egmPriceModifiers.Any())
            {
                return game.PricePerGame;
            }
            bool hasModifier = false;
            //ScheduleDay modifier;
            PriceChangerEnum sign = PriceChangerEnum.None;
            decimal percents = 0m;

            // get modifier
            foreach (var priceModifier in egmPriceModifiers) 
            {
                bool isModifierValid = priceModifier.PriceChangerSign != PriceChangerEnum.None && priceModifier.PriceModifier > 0;
                if (priceModifier.IsActive && priceModifier.IsDateRange)
                {
                    if (DateTime.UtcNow >= priceModifier.From && DateTime.UtcNow <= priceModifier.To)
                        TrySetModifier(isModifierValid, out hasModifier, out sign, out percents, priceModifier);
                    if (hasModifier)
                        break;
                }
                else if (priceModifier.IsActive)
                {
                    var currentDay = (int)DateTime.UtcNow.DayOfWeek;
                    if (currentDay == 0)
                        currentDay = 7;
                    if (currentDay == priceModifier.DayOfWeek)
                    {
                        TrySetModifier(isModifierValid, out hasModifier, out sign, out percents, priceModifier);
                        if (hasModifier)
                            break;
                    }
                }
            }
            // modified price per game
            var pricePerGame = game.PricePerGame;
            if (hasModifier) 
            {
                if (sign == PriceChangerEnum.Add)
                    pricePerGame += ((pricePerGame / 100) * percents);
                else if (sign == PriceChangerEnum.Subtract)
                    pricePerGame -= ((pricePerGame / 100) * percents);
            }
            return pricePerGame;
        }

        private Result<Card> UpdateCurrency(IServiceProvider serviceProvider, Card editedCard) 
        {
            Result<Card> result = new Result<Card>();
            try 
            {
                ICardSystemRepository<Card> repo = serviceProvider.GetService<ICardSystemRepository<Card>>();
                Card entity = repo.FirstOrDefault(x => x.Id == editedCard.Id);
                if (entity == null)
                {
                    _logger.LogException("BusinessServices.CanPlayService.UpdateCurrency => Entity not found!");
                    result.Messages.Add("Entity not found!");
                    result.IsSuccess = false;
                    return result;
                }
                if (entity.ExternalId != editedCard.ExternalId)
                {
                    if (!IsExternalIdUnique(serviceProvider, editedCard.ExternalId, entity.Id))
                    {
                        _logger.LogException("BusinessServices.CanPlayService.UpdateCurrency => External ID is not UNIQUE!");
                        result.Messages.Add("External ID is not UNIQUE!");
                        result.IsSuccess = false;
                        return result;
                    }
                }
                //Currency1
                entity.Currency1Balance = editedCard.Currency1Balance;
                entity.Currency1BalanceBonus = editedCard.Currency1BalanceBonus;
                entity.Currency1Code = editedCard.Currency1Code;
                entity.Currency1Name = editedCard.Currency1Name;
                entity.Currency1TotalIncome = editedCard.Currency1TotalIncome;
                entity.Currency1TotalSpend = editedCard.Currency1TotalSpend;
                //CurrencyBonus
                entity.CurrencyBonusBalance = editedCard.CurrencyBonusBalance;
                entity.CurrencyBonusBalanceBonus = editedCard.CurrencyBonusBalanceBonus;
                entity.CurrencyBonusCode = editedCard.CurrencyBonusCode;
                entity.CurrencyBonusName = editedCard.CurrencyBonusName;
                entity.CurrencyBonusTotalIncome = editedCard.CurrencyBonusTotalIncome;
                entity.CurrencyBonusTotalSpend = editedCard.CurrencyBonusTotalSpend;
                //CurrencyCredit
                entity.CurrencyCreditBalance = editedCard.CurrencyCreditBalance;
                entity.CurrencyCreditBalanceBonus = editedCard.CurrencyCreditBalanceBonus;
                entity.CurrencyCreditCode = editedCard.CurrencyCreditCode;
                entity.CurrencyCreditName = editedCard.CurrencyCreditName;
                entity.CurrencyCreditTotalIncome = editedCard.CurrencyCreditTotalIncome;
                entity.CurrencyCreditTotalSpend = editedCard.CurrencyCreditTotalSpend;
                //CurrencyPrimary
                entity.CurrencyPrimaryBalance = editedCard.CurrencyPrimaryBalance;
                entity.CurrencyPrimaryBalanceBonus = editedCard.CurrencyPrimaryBalanceBonus;
                entity.CurrencyPrimaryCode = editedCard.CurrencyPrimaryCode;
                entity.CurrencyPrimaryName = editedCard.CurrencyPrimaryName;
                entity.CurrencyPrimaryTotalIncome = editedCard.CurrencyPrimaryTotalIncome;
                entity.CurrencyPrimaryTotalSpend = editedCard.CurrencyPrimaryTotalSpend;
                //CurrencyTicket
                entity.CurrencyTicketBalance = editedCard.CurrencyTicketBalance;
                entity.CurrencyTicketBalanceBonus = editedCard.CurrencyTicketBalanceBonus;
                entity.CurrencyTicketCode = editedCard.CurrencyTicketCode;
                entity.CurrencyTicketName = editedCard.CurrencyTicketName;
                entity.CurrencyTicketTotalIncome = editedCard.CurrencyTicketTotalIncome;
                entity.CurrencyTicketTotalSpend = editedCard.CurrencyTicketTotalSpend;
                //CurrencyTime
                entity.CurrencyTimeBalance = editedCard.CurrencyTimeBalance;
                entity.CurrencyTimeBalanceBonus = editedCard.CurrencyTimeBalanceBonus;
                entity.CurrencyTimeCode = editedCard.CurrencyTimeCode;
                entity.CurrencyTimeName = editedCard.CurrencyTimeName;
                entity.CurrencyTimeTotalIncome = editedCard.CurrencyTimeTotalIncome;
                entity.CurrencyTimeTotalSpend = editedCard.CurrencyTimeTotalSpend;

                entity.FreePeriod = editedCard.FreePeriod;
                entity.FreePeriodStart = editedCard.FreePeriodStart;
                entity.FreePeriodEnd = editedCard.FreePeriodEnd;
                entity.LastTransactionTime = DateTime.UtcNow;
                repo.Update();

                // update cache
                Card cachedEntity = CardCacheServiceV2.Instance.GetCard(serviceProvider, entity.Id);
                if (cachedEntity != null) 
                {
                    //Currency1
                    cachedEntity.Currency1Balance = entity.Currency1Balance;
                    cachedEntity.Currency1BalanceBonus = entity.Currency1BalanceBonus;
                    cachedEntity.Currency1Code = entity.Currency1Code;
                    cachedEntity.Currency1Name = entity.Currency1Name;
                    cachedEntity.Currency1TotalIncome = entity.Currency1TotalIncome;
                    cachedEntity.Currency1TotalSpend = entity.Currency1TotalSpend;
                    //CurrencyBonus
                    cachedEntity.CurrencyBonusBalance = entity.CurrencyBonusBalance;
                    cachedEntity.CurrencyBonusBalanceBonus = entity.CurrencyBonusBalanceBonus;
                    cachedEntity.CurrencyBonusCode = entity.CurrencyBonusCode;
                    cachedEntity.CurrencyBonusName = entity.CurrencyBonusName;
                    cachedEntity.CurrencyBonusTotalIncome = entity.CurrencyBonusTotalIncome;
                    cachedEntity.CurrencyBonusTotalSpend = entity.CurrencyBonusTotalSpend;
                    //CurrencyCredit
                    cachedEntity.CurrencyCreditBalance = entity.CurrencyCreditBalance;
                    cachedEntity.CurrencyCreditBalanceBonus = entity.CurrencyCreditBalanceBonus;
                    cachedEntity.CurrencyCreditCode = entity.CurrencyCreditCode;
                    cachedEntity.CurrencyCreditName = entity.CurrencyCreditName;
                    cachedEntity.CurrencyCreditTotalIncome = entity.CurrencyCreditTotalIncome;
                    cachedEntity.CurrencyCreditTotalSpend = entity.CurrencyCreditTotalSpend;
                    //CurrencyPrimary
                    cachedEntity.CurrencyPrimaryBalance = entity.CurrencyPrimaryBalance;
                    cachedEntity.CurrencyPrimaryBalanceBonus = entity.CurrencyPrimaryBalanceBonus;
                    cachedEntity.CurrencyPrimaryCode = entity.CurrencyPrimaryCode;
                    cachedEntity.CurrencyPrimaryName = entity.CurrencyPrimaryName;
                    cachedEntity.CurrencyPrimaryTotalIncome = entity.CurrencyPrimaryTotalIncome;
                    cachedEntity.CurrencyPrimaryTotalSpend = entity.CurrencyPrimaryTotalSpend;
                    //CurrencyTicket
                    cachedEntity.CurrencyTicketBalance = entity.CurrencyTicketBalance;
                    cachedEntity.CurrencyTicketBalanceBonus = entity.CurrencyTicketBalanceBonus;
                    cachedEntity.CurrencyTicketCode = entity.CurrencyTicketCode;
                    cachedEntity.CurrencyTicketName = entity.CurrencyTicketName;
                    cachedEntity.CurrencyTicketTotalIncome = entity.CurrencyTicketTotalIncome;
                    cachedEntity.CurrencyTicketTotalSpend = entity.CurrencyTicketTotalSpend;
                    //CurrencyTime
                    cachedEntity.CurrencyTimeBalance = entity.CurrencyTimeBalance;
                    cachedEntity.CurrencyTimeBalanceBonus = entity.CurrencyTimeBalanceBonus;
                    cachedEntity.CurrencyTimeCode = entity.CurrencyTimeCode;
                    cachedEntity.CurrencyTimeName = entity.CurrencyTimeName;
                    cachedEntity.CurrencyTimeTotalIncome = entity.CurrencyTimeTotalIncome;
                    cachedEntity.CurrencyTimeTotalSpend = entity.CurrencyTimeTotalSpend;

                    cachedEntity.FreePeriod = entity.FreePeriod;
                    cachedEntity.FreePeriodStart = entity.FreePeriodStart;
                    cachedEntity.FreePeriodEnd = entity.FreePeriodEnd;
                    cachedEntity.LastTransactionTime = entity.LastTransactionTime;
                    CardCacheServiceV2.Instance.AddCard(serviceProvider, cachedEntity);
                    CardCacheServiceV2.Instance.AddNewMap(cachedEntity);
                }

                result.Data = entity;
                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogException($"BusinessServices.CanPlayService.UpdateCurrency throws: {ex.Message}");
                result.Messages.Add($"Cannot edit card currencies! Something went wrong. {ex.Message}");
                result.IsSuccess = false;

                return result;
            }
        }

        private bool FirstPayFromBase
        {
            get
            {
                return _configuration.GetSection("Config:FirstPayFromBase").Get<bool>();
            }
        }
        
        private bool IsCardValidV2(Card card)
        {
            if (card == null)
            {
                return false;
            }

            if (card.ActivatedOn > DateTime.UtcNow)
            {
                return false;
            }

            if (card.ExpirationDate < DateTime.UtcNow)
            {
                return false;
            }

            if (card.State != CState.Activated)
            {
                return false;
            }

            if (!card.IsActive)
            {
                return false;
            }

            return true;
        }
        
        private void TrySetModifier(
            bool isModifierValid, 
            out bool hasModifier, 
            out PriceChangerEnum sign, 
            out decimal percents, 
            ScheduleDay priceModifier) 
        {
            if (isModifierValid)
            {
                hasModifier = true;
                sign = priceModifier.PriceChangerSign;
                percents = priceModifier.PriceModifier;
            }
            else
            {
                hasModifier = false;
                sign = PriceChangerEnum.None;
                percents = 0m;
            }

            var hourEntities = priceModifier.Hours
                .Where(x => x.IsActive)
                .OrderBy(x => x.StartFromHour)
                .ThenBy(x => x.StartFromMinutes);

            foreach (var hour in hourEntities)
            {
                var startTime = new DateTime(
                    DateTime.UtcNow.Year, 
                    DateTime.UtcNow.Month, 
                    DateTime.UtcNow.Day, 
                    hour.StartFromHour, 
                    hour.StartFromMinutes, 
                    0).ToUniversalTime();
                var endTime = new DateTime(
                    DateTime.UtcNow.Year, 
                    DateTime.UtcNow.Month, 
                    DateTime.UtcNow.Day, 
                    hour.EndAtHour, 
                    hour.EndAtMinutes, 
                    0).ToUniversalTime();

                bool inTime = DateTime.UtcNow >= startTime && DateTime.UtcNow <= endTime;

                bool hasHourModifier = hour.PriceChangerSign != PriceChangerEnum.None && hour.PriceModifier > 0;

                if (hour.IsActive && inTime && hasHourModifier)
                {
                    hasModifier = true;
                    sign = hour.PriceChangerSign;
                    percents = hour.PriceModifier;
                    break;
                }
            }
        }

        private decimal CalculateByDiscount(
            decimal currentPrice, 
            int discount)
        {
            decimal price;
            if (discount > 0)
            {
                price = currentPrice * (1 + (discount / 100m));
                return price;
            }
            price = currentPrice * (1 - Math.Abs(discount) / 100m);
            return price;
        }

        private decimal GetBalanceByCurrencyType(Card card, string currencyType)
        {
            if (currencyType == CCurrency.Credit)
            {
                return card.CurrencyPrimaryBalance + card.CurrencyPrimaryBalanceBonus;
            }
            else if (currencyType == CCurrency.Courtesy)
            {
                return card.CurrencyCreditBalance + card.CurrencyCreditBalanceBonus;
            }
            else if (currencyType == CCurrency.Bonus)
            {
                return card.CurrencyBonusBalance + card.CurrencyBonusBalanceBonus;
            }
            else if (currencyType == CCurrency.TicketBonus)
            {
                return card.CurrencyTicketBalance + card.CurrencyTicketBalanceBonus;
            }
            else if (currencyType == CCurrency.Minutes)
            {
                return card.CurrencyTimeBalance + card.CurrencyTimeBalanceBonus;
            }
            else if (currencyType == CCurrency.TicketUsedBonus)
            {
                return card.Currency1Balance + card.Currency1BalanceBonus;
            }
            else
            {
                return 0;
            }
        }

        private (decimal paidFromPrimary, decimal paidFromBonus) PaymentByCurrencyType(
            Card card, 
            string currencyType, 
            decimal pricePerGame)
        {
            decimal paidFromPrimary = 0;
            decimal paidFromBonus = 0;

            // balances
            decimal primary = 0;
            decimal bonus = 0;

            if (currencyType == CCurrency.Credit)
            {
                (primary, bonus, paidFromPrimary, paidFromBonus) =
                    this.Payment(card.CurrencyPrimaryBalance, card.CurrencyPrimaryBalanceBonus, pricePerGame);

                card.CurrencyPrimaryBalance = primary;
                card.CurrencyPrimaryBalanceBonus = bonus;
                card.CurrencyPrimaryTotalSpend += paidFromPrimary + paidFromBonus;
            }
            else if (currencyType == CCurrency.Courtesy)
            {
                (primary, bonus, paidFromPrimary, paidFromBonus) =
                    this.Payment(card.CurrencyCreditBalance, card.CurrencyCreditBalanceBonus, pricePerGame);

                card.CurrencyCreditBalance = primary;
                card.CurrencyCreditBalanceBonus = bonus;
                card.CurrencyCreditTotalSpend += paidFromPrimary + paidFromBonus;
            }
            else if (currencyType == CCurrency.Bonus)
            {
                (primary, bonus, paidFromPrimary, paidFromBonus) =
                    this.Payment(card.CurrencyBonusBalance, card.CurrencyBonusBalanceBonus, pricePerGame);

                card.CurrencyBonusBalance = primary;
                card.CurrencyBonusBalanceBonus = bonus;
                card.CurrencyBonusTotalSpend += paidFromPrimary + paidFromBonus;
            }
            else if (currencyType == CCurrency.TicketBonus)
            {
                (primary, bonus, paidFromPrimary, paidFromBonus) =
                    this.Payment(card.CurrencyTicketBalance, card.CurrencyTicketBalanceBonus, pricePerGame);

                card.CurrencyTicketBalance = primary;
                card.CurrencyTicketBalanceBonus = bonus;
                card.CurrencyTicketTotalSpend += paidFromPrimary + paidFromBonus;
            }
            else if (currencyType == CCurrency.Minutes)
            {
                (primary, bonus, paidFromPrimary, paidFromBonus) =
                    this.Payment(card.CurrencyTimeBalance, card.CurrencyTimeBalanceBonus, pricePerGame);

                card.CurrencyTimeBalance = primary;
                card.CurrencyTimeBalanceBonus = bonus;
                card.CurrencyTimeTotalSpend += paidFromPrimary + paidFromBonus;
            }
            else if (currencyType == CCurrency.TicketUsedBonus)
            {
                (primary, bonus, paidFromPrimary, paidFromBonus) =
                    this.Payment(card.Currency1Balance, card.Currency1BalanceBonus, pricePerGame);

                card.Currency1Balance = primary;
                card.Currency1BalanceBonus = bonus;
                card.Currency1TotalSpend += paidFromPrimary + paidFromBonus;
            }

            return (paidFromPrimary, paidFromBonus);

        }

        private (decimal resultPrimary, decimal resultBonus, decimal paidFromPrimary, decimal paidFromBonus) Payment(
            decimal primary, 
            decimal bonus, 
            decimal pricePerGame)
        {
            decimal paidFromPrimary = 0;
            decimal paidFromBonus = 0;

            if (this.FirstPayFromBase)
            {
                if (primary >= pricePerGame)
                {
                    primary -= pricePerGame;
                    paidFromPrimary = pricePerGame;
                }
                else if (primary > 0)
                {
                    decimal temp = primary;
                    primary = 0;
                    bonus -= (pricePerGame - temp);

                    paidFromBonus = pricePerGame - temp;
                    paidFromPrimary = temp;
                }
                else
                {
                    bonus -= pricePerGame;
                    paidFromBonus = pricePerGame;
                }
            }
            else
            {
                if (bonus >= pricePerGame)
                {
                    bonus -= pricePerGame;
                    paidFromBonus = pricePerGame;// result.PaidFromBonusBalance = pricePerGame;
                }
                else if (bonus > 0)
                {
                    decimal temp = bonus; // tempCurrency.BonusBalance;
                    bonus = 0; // tempCurrency.PayFromBonus(temp);
                    primary -= (pricePerGame - temp); // tempCurrency.Pay(pricePerGame - temp);

                    paidFromPrimary = pricePerGame - temp; // result.PaidFromPrimaryBalance = pricePerGame - temp;
                    paidFromBonus = temp; // result.PaidFromBonusBalance = temp;
                }
                else
                {
                    primary -= pricePerGame; // tempCurrency.Pay(pricePerGame);
                    paidFromPrimary = pricePerGame;// result.PaidFromPrimaryBalance = pricePerGame;
                }
            }

            return (primary, bonus, paidFromPrimary, paidFromBonus);
        }

        private bool IsExternalIdUnique(IServiceProvider serviceProvider, string externalId, string cardId)
        {
            if (string.IsNullOrWhiteSpace(externalId))
            {
                return false;
            }

            Card entity = GetByExternalId(serviceProvider, externalId);
            bool isUnique = entity == null;

            if (entity != null && !string.IsNullOrWhiteSpace(cardId))
            {
                isUnique = entity.Id == cardId;
            }

            return isUnique;
        }

        private Card GetByExternalId(IServiceProvider serviceProvider, string externalId)
        {
            Card entity = null;
            ICardSystemRepository<Card> repo = serviceProvider.GetService<ICardSystemRepository<Card>>();

            string cardId = CardCacheServiceV2.Instance.GetIdByExternalId(externalId);
            if (!string.IsNullOrWhiteSpace(cardId))
            {
                entity = CardCacheServiceV2.Instance.GetCard(serviceProvider, cardId);
                if (entity != null)
                {
                    return entity;
                }
                entity = repo.AsNoTracking()
                             .Include(x => x.CardMode)
                             .Include(x => x.User)
                             .FirstOrDefault(x => x.Id == cardId);
                if (entity != null)
                {
                    CardCacheServiceV2.Instance.AddCard(serviceProvider, entity);
                    CardCacheServiceV2.Instance.AddNewMap(entity);
                }
                return entity;
            }

            entity = repo.AsNoTracking()
                        .Include(x => x.CardMode)
                        .Include(x => x.User)
                        .FirstOrDefault(x => x.ExternalId == externalId);
            // add to cache
            if (entity != null)
            {
                CardCacheServiceV2.Instance.AddCard(serviceProvider, entity);
                CardCacheServiceV2.Instance.AddNewMap(entity);
            }
            return entity;
        }
    }
}
