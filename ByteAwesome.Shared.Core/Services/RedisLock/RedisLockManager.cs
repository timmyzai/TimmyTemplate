using Polly.Retry;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace ByteAwesome.Services
{
    public interface IRedisLockManager
    {
        Task<IDisposable> WaitAsync(string key, TimeSpan? expiry = null);
        Task<bool> ReleaseAsync(IDisposable redisLock);
    }

    public class RedisLockManager : IRedisLockManager
    {
        private readonly RedLockFactory _redLockFactory;
        private readonly AsyncRetryPolicy<IRedLock> _retryPolicy;

        public RedisLockManager(IConnectionMultiplexer redis)
        {
            var configurationOptions = ConfigurationOptions.Parse(redis.Configuration);
            var endPoints = redis.GetEndPoints();
            var redisLockEndPoints = endPoints.Select(endpoint =>
                new RedLockEndPoint
                {
                    EndPoint = endpoint,
                    Password = configurationOptions.Password
                }).ToList();
            _redLockFactory = RedLockFactory.Create(redisLockEndPoints);
            _retryPolicy = PolicyBuilder.CreateRedisLockRetryPolicy(retryCount: 5);
        }

        public async Task<IDisposable> WaitAsync(string key, TimeSpan? expiry = null)
        {
            expiry ??= TimeSpan.FromMinutes(2);

            IRedLock redisLock = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _redLockFactory.CreateLockAsync(key, expiry.Value);
            });

            if (!redisLock.IsAcquired)
            {
                throw new Exception("Failed to acquire Redis lock after multiple retries.");
            }

            return redisLock;
        }

        public async Task<bool> ReleaseAsync(IDisposable redisLock)
        {
            redisLock?.Dispose();
            return true;
        }
    }
}
