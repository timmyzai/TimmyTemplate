using Newtonsoft.Json;
using StackExchange.Redis;

namespace ByteAwesome.Services
{
    public interface IRedisCacheService
    {
        Task DeleteAsync(string key);
        Task<string> GetAsync(string key, bool throwOnNotFound = false);
        Task<T> GetAsync<T>(string key, bool throwOnNotFound = false);
        Task SetAsync(string key, string value, TimeSpan expiry);
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private readonly ICacheService cacheService;
        private bool _isRedisConnected;
        public RedisCacheService
        (
            IConnectionMultiplexer connectionMultiplexer,
            ICacheService cacheService
        )
        {
            try
            {
                _database = connectionMultiplexer.GetDatabase();
                _isRedisConnected = true;
            }
            catch
            {
                _isRedisConnected = false;
            }
            this.cacheService = cacheService;
        }

        public async Task<string> GetAsync(string key, bool throwOnNotFound = false)
        {
            if (_isRedisConnected)
            {
                RedisValue result = await _database.StringGetAsync(key);
                if (throwOnNotFound && (!result.HasValue || result.IsNull))
                {
                    throw new Exception($"{key} not found");
                }
                return result.ToString();
            }
            else
            {
                cacheService.TryGetValue(key, out string value);
                return value;
            }
        }
        public async Task<T> GetAsync<T>(string key, bool throwOnNotFound = false)
        {
            if (_isRedisConnected)
            {
                RedisValue result = await _database.StringGetAsync(key);
                if (throwOnNotFound && (!result.HasValue || result.IsNull))
                {
                    throw new Exception($"{key} not found");
                }
                return JsonConvert.DeserializeObject<T>(result.ToString());
            }
            else
            {
                cacheService.TryGetValue(key, out string value);
                return JsonConvert.DeserializeObject<T>(value);
            }
        }
        public async Task SetAsync(string key, string value, TimeSpan expiry)
        {
            if (_isRedisConnected)
            {
                await _database.StringSetAsync(key, value, expiry);
            }
            else
            {
                cacheService.Set(key, value, expiry);
            }
        }
        public async Task DeleteAsync(string key)
        {
            if (_isRedisConnected)
            {
                if (await _database.KeyExistsAsync(key))
                {
                    await _database.KeyDeleteAsync(key);
                }
            }
            else
            {
                cacheService.Remove(key);
            }
        }
    }
}