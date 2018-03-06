using System;
using System.Runtime.Caching;

namespace MusicGenie
{
    public class InMemoryCache : ICacheService
    {
        private readonly int _cacheTimeMinutes;

        public InMemoryCache(int cacheTimeMinutes)
        {
            _cacheTimeMinutes = cacheTimeMinutes;
        }

        public T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class
        {
            T item = MemoryCache.Default.Get(cacheKey) as T;
            if (item == null)
            {
                item = getItemCallback();
                MemoryCache.Default.Add(cacheKey, item, DateTime.Now.AddMinutes(_cacheTimeMinutes));
            }
            return item;
        }
    }

    public interface ICacheService
    {
        T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class;
    }
}