using StackExchange.Redis;

namespace ByteAwesome.Services
{
    public interface IRedisCacheService
    {
        Task<string> GetAsync(string key);
        Task SetAsync(string key, string value, TimeSpan expiry);
    }
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<string> GetAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public async Task SetAsync(string key, string value, TimeSpan expiry)
        {
            await _database.StringSetAsync(key, value, expiry);
        }
    }
}