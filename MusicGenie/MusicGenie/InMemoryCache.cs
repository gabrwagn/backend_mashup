using System;
using System.Runtime.Caching;

namespace MusicGenie
{
    public class InMemoryCache : ICacheService
    {
        private readonly int _cacheTimeMinutes;
        private readonly MemoryCache _cache;

        public InMemoryCache(int cacheTimeMinutes, MemoryCache cache)
        {
            _cacheTimeMinutes = cacheTimeMinutes;
            _cache = cache;
        }
        public InMemoryCache(int cacheTimeMinutes)
            :this(cacheTimeMinutes, MemoryCache.Default)
        {

        }


        public T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class
        {
            T item = _cache.Get(cacheKey) as T;
            if (item == null)
            {
                item = getItemCallback();
                _cache.Add(cacheKey, item, DateTime.Now.AddMinutes(_cacheTimeMinutes));
            }
           
            return item;
        }

        public T Get<T>(string cacheKey) where T : class
        {
            T item = _cache.Get(cacheKey) as T;
            return item;
        }

        public void Set<T>(string cacheKey, T item) where T : class
        {
            _cache.Add(cacheKey, item, DateTime.Now.AddMinutes(_cacheTimeMinutes));
        }
    }

    public interface ICacheService
    {
        T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class;
    }
}