using gRPCNet.ServerAPI.CommonServices.Utils;
using gRPCNet.ServerAPI.DAL.Repositories;
using gRPCNet.ServerAPI.Models.Domain.Cards;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace gRPCNet.ServerAPI.BusinessServices
{
    public sealed class CardCacheServiceV2
    {
        private static readonly object _syncRoot = new object();
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        private static volatile CardCacheServiceV2 _instance;
        private static ConcurrentDictionary<string, string> _numberIdDict;
        private static ConcurrentDictionary<string, string> _externalIdDict;
        private static int _cacheExpirationTimeout;
        private static bool _isSlidingExpiration;

        private CardCacheServiceV2() { }

        public static CardCacheServiceV2 Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new CardCacheServiceV2();
                    }
                }

                return _instance;
            }
        }
        /// <summary>
        /// Инициализира _numberIdDict и _externalIdDict и ги зарежда с идентификаторите на всички карти от БД 
        /// </summary>
        /// <param name="serviceProvider">имплементация на IServiceProvider</param>
        /// <param name="cacheExpirationTimeout">време в секунди за което картите са налични в кеша</param>
        public void Init(IServiceProvider serviceProvider, int cacheExpirationTimeout, bool isSlidingExpiration) 
        {
            try
            {
                _semaphoreSlim.Wait();
                //
                _numberIdDict = new ConcurrentDictionary<string, string>();
                _externalIdDict = new ConcurrentDictionary<string, string>();
                _cacheExpirationTimeout = cacheExpirationTimeout;
                _isSlidingExpiration = isSlidingExpiration;

                ICardSystemRepository<Card> repo = serviceProvider.GetService<ICardSystemRepository<Card>>();
                ILogger logger = new Logger();
                int count = repo.AsNoTracking().Count();
                int step = 40000;
                int iterations = count / step;
                for (int i = 0; i <= iterations; i++)
                {
                    var cardIds = repo.AsNoTracking()
                        .Select(c => new { c.Id, c.Number, c.ExternalId })
                        .Skip(i * step)
                        .Take(step)
                        .AsEnumerable()
                        .Select(c => (c.Id, c.Number, c.ExternalId))
                        .ToArray();
                    foreach (var item in cardIds)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Number) && !_numberIdDict.ContainsKey(item.Number))
                            _numberIdDict.AddOrUpdate(item.Number, item.Id, (oldNum, oldId) => item.Id);
                        if (!string.IsNullOrWhiteSpace(item.ExternalId) && !_externalIdDict.ContainsKey(item.ExternalId))
                            _externalIdDict.AddOrUpdate(item.ExternalId, item.Id, (oldNum, oldId) => item.Id);
                    }
                    logger.InfoLog($"Successfully load into card dictionaries mapping cache {cardIds.Length} cards");
                }
                // to do ...
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        /// <summary>
        /// Извлича Id на карта от ConcurrentDictionary(_numberIdDict) по number
        /// </summary>
        /// <param name="number">card's number</param>
        /// <returns>Id или string.Empty</returns>
        public string GetIdByNumber(string number)
        {
            string id = string.Empty;
            if (_numberIdDict.ContainsKey(number))
            {
                _numberIdDict.TryGetValue(number, out id);
            }
            return id;
        }
        /// <summary>
        /// Извлича Id на карта от ConcurrentDictionary(_externalIdDict) по  externalId
        /// </summary>
        /// <param name="externalId">card's external Id</param>
        /// <returns>Id или string.Empty</returns>
        public string GetIdByExternalId(string externalId)
        {
            string id = string.Empty;
            if (_externalIdDict.ContainsKey(externalId))
            {
                _externalIdDict.TryGetValue(externalId, out id);
            }
            return id;
        }
        /// <summary>
        /// Добавя или обновява запис в _numberIdDict и _externalIdDict
        /// </summary>
        /// <param name="card"></param>
        public void AddNewMap(Card card)
        {
            try
            {
                _semaphoreSlim.Wait();
                string id = card.Id;
                _numberIdDict.AddOrUpdate(card.Number, id, (oldNum, oldId) => id);
                _externalIdDict.AddOrUpdate(card.ExternalId, id, (oldNum, oldId) => id);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        /// <summary>
        /// Премахва запис от _numberIdDict и _externalIdDict
        /// </summary>
        public void RemoveMap(string number, string externalId)
        {
            try
            {
                _semaphoreSlim.Wait();
                _numberIdDict.TryRemove(number, out string id);
                _externalIdDict.TryRemove(externalId, out id);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        /// <summary>
        /// Премахва картите от кеша
        /// </summary>
        /// <param name="serviceProvider">имплементация на IServiceProvider</param>
        /// <param name="cards">списък от карти</param>
        public void BulkRemoveCards(IServiceProvider serviceProvider, IList<Card> cards)
        {
            _semaphoreSlim.Wait();
            try
            {
                IMemoryCache cache = serviceProvider.GetService<IMemoryCache>();
                foreach (var card in cards)
                    cache.Remove(card.Id);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        /// <summary>
        /// Добавя или обновява картата в кеша
        /// </summary>
        /// <param name="serviceProvider">имплементация на IServiceProvider</param>
        /// <param name="card">инстанция на Card</param>
        public void AddCard(IServiceProvider serviceProvider, Card card)
        {
            if (card != null)
            {
                _semaphoreSlim.Wait();
                try
                {
                    IMemoryCache cache = serviceProvider.GetService<IMemoryCache>();
                    var cacheEntryOptions = _isSlidingExpiration ?
                        new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(_cacheExpirationTimeout)) :
                        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheExpirationTimeout));
                    cache.Set(card.Id, card, cacheEntryOptions);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
        }
        /// <summary>
        /// Извлича карта от кеша по cardId.
        /// </summary>
        /// <param name="serviceProvider">имплементация на IServiceProvider</param>
        /// <param name="cardId">Id на картата</param>
        /// <returns>Card или null</returns>
        public Card GetCard(IServiceProvider serviceProvider, string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
            {
                return null;
            }
            IMemoryCache cache = serviceProvider.GetService<IMemoryCache>();
            Card card;
            //CHECK - optimistic
            if (cache.TryGetValue(cardId, out card))
            {
                return card;
            }
            _semaphoreSlim.Wait();
            try
            {
                //CHECK - pessimistic
                if (cache.TryGetValue(cardId, out card))
                {
                    return card;
                }
                return null;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
