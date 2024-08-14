using Newtonsoft.Json;
using StackExchange.Redis;
using Polly.Retry;
using Serilog;

namespace ByteAwesome.Services
{
    public interface IRedisCacheService
    {
        Task DeleteAsync(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiry);
        Task<T> GetAsync<T>(string key, bool throwOnNotFound = false);
        Task<T> GetAndSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiry) where T : class;
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _database = _connectionMultiplexer.GetDatabase();
            _retryPolicy = PolicyBuilder.CreateRetry<RedisException>();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry)
        {
            if (!_connectionMultiplexer.IsConnected) return;
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var jsonData = JsonConvert.SerializeObject(value);
                bool setResult = await _database.StringSetAsync(key, jsonData, expiry);
                if (!setResult)
                {
                    throw new InvalidOperationException("Failed to set the cache key.");
                }
            });
        }

        public async Task DeleteAsync(string key)
        {
            if (!_connectionMultiplexer.IsConnected) return;
            await _retryPolicy.ExecuteAsync(async () =>
            {
                bool exists = await _database.KeyExistsAsync(key);
                if (!exists || !await _database.KeyDeleteAsync(key))
                {
                    Log.Warning("Failed to delete key: {Key} or key does not exist.", key);
                }
            });
        }

        public async Task<T> GetAsync<T>(string key, bool throwOnNotFound = false)
        {
            if (!_connectionMultiplexer.IsConnected) return default;
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                RedisValue value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                {
                    if (throwOnNotFound)
                        throw new KeyNotFoundException($"Key '{key}' not found.");
                    return default;
                }
                return JsonConvert.DeserializeObject<T>(value);
            });
        }

        public async Task<T> GetAndSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiry) where T : class
        {
            if (!_connectionMultiplexer.IsConnected) return await factory();
            T result = null;
            try
            {
                result = await GetAsync<T>(key);
                if (result is not null) return result;
                result = await factory();
                await SetAsync(key, result, expiry);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetAndSetAsync while handling the key: {Key}", key);
                result ??= await factory();
                Log.Information("Fallback executed for key: {Key}", key);
            }
            return result;
        }
    }
}
