using gRPCNet.Proto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace gRPCNet.ServerAPI.BusinessServices
{
    public sealed class CanPlayCacheService
    {
        private static readonly object _syncRoot = new object();

        private static volatile CanPlayCacheService _instance;
        private static IMemoryCache _cache;
        private static int _cacheExpirationTimeout;
        private static bool _isSlidingExpiration;

        private CanPlayCacheService() { }

        public static CanPlayCacheService Instance 
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new CanPlayCacheService();
                    }
                }

                return _instance;
            }
        }

        public void Init(IServiceProvider serviceProvider, int cacheExpirationTimeout, bool isSlidingExpiration)
        {
            _cacheExpirationTimeout = cacheExpirationTimeout;
            _isSlidingExpiration = isSlidingExpiration;
            _cache = serviceProvider.GetService<IMemoryCache>();
        }
        /// <summary>
        /// Добавя или обновява резултата в кеша
        /// </summary>
        /// <param name="key">ключ</param>
        /// <param name="item">резултат</param>
        /// <param name="threadSafe">флаг за използване на mutex(lock). Ако операцията се извършва в семафор трябва да е false</param>
        public void Add(string key, (string key, CanPlayResponse successResponse) item, bool threadSafe = false) 
        {
            var cacheEntryOptions = _isSlidingExpiration ?
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(_cacheExpirationTimeout)) :
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheExpirationTimeout));
            if (threadSafe)
                lock (_syncRoot)
                {
                    _cache.Set(key, item, cacheEntryOptions);
                }
            else
                _cache.Set(key, item, cacheEntryOptions);
        }
        /// <summary>
        /// Извлича резултат от кеша
        /// </summary>
        /// <param name="key">ключ</param>
        /// <param name="threadSafe">флаг за използване на mutex(lock). Ако операцията се извършва в семафор трябва да е false</param>
        /// <returns>резултата ако има такъв ключ или тюпъл (string.Empty, null)</returns>
        public (string key, CanPlayResponse successResponse) Get(string key, bool threadSafe = false) 
        {
            if (threadSafe)
            {
                lock (_syncRoot)
                {
                    if (_cache.TryGetValue(key, out (string key, CanPlayResponse successResponse) value))
                        return value;
                }
                return (string.Empty, null);
            }
            else
            {
                if (_cache.TryGetValue(key, out (string key, CanPlayResponse successResponse) value))
                    return value;
                return (string.Empty, null);
            }
        }
    }
}
