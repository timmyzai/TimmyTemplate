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
        private readonly RedLockFactory redLockFactory;

        public RedisLockManager(IConnectionMultiplexer redis)
        {
            var endPoints = redis.GetEndPoints();
            var redisLockEndPoints = endPoints.Select(endpoint => new RedLockEndPoint { EndPoint = endpoint }).ToList();
            this.redLockFactory = RedLockFactory.Create(redisLockEndPoints);
        }

        public async Task<IDisposable> WaitAsync(string key, TimeSpan? expiry = null)
        {
            expiry = expiry ?? TimeSpan.FromMinutes(2);
            IRedLock redisLock;
            do
            {
                redisLock = await this.redLockFactory.CreateLockAsync(key, expiry.Value);
                if (!redisLock.IsAcquired)
                {
                    await Task.Delay(500);
                }
            }
            while (!redisLock.IsAcquired);
            return redisLock;
        }
        public async Task<bool> ReleaseAsync(IDisposable redisLock)
        {
            redisLock?.Dispose();
            return true;
        }
    }
}