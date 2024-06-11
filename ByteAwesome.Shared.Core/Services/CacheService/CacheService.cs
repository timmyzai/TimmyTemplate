using Microsoft.Extensions.Caching.Memory;

namespace ByteAwesome.Services
{
    public interface ICacheService
    {
        void Set<T>(string key, T value, TimeSpan expirationTime);
        bool TryGetValue<T>(string key, out T value);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public void Set<T>(string key, T value, TimeSpan expirationTime)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expirationTime);
            _cache.Set(key, value, cacheEntryOptions);
        }
    }
}