using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Serilog;

namespace ByteAwesome
{
    public static class RedisConnectionManager
    {
        public static void ConnectRedis(IServiceCollection services, string redisConnectionString)
        {
            try
            {
                var redisConfig = ConfigurationOptions.Parse(redisConnectionString);
                redisConfig.AbortOnConnectFail = false; // Do not throw exception on initial connection failure
                redisConfig.ConnectRetry = 5; // Retry connection 5 times
                redisConfig.ConnectTimeout = 5000; // Connection timeout of 5 seconds
                redisConfig.SyncTimeout = 5000; // Synchronous operation timeout of 5 seconds
                redisConfig.ReconnectRetryPolicy = new ExponentialRetry(2000, 30000); // 2 secs initial backoff, 30 secs max backoff

                var redis = ConnectionMultiplexer.Connect(redisConfig);

                redis.ConnectionRestored += (sender, args) =>
                {
                    Log.Information($"Redis connection restored: {args.EndPoint}");
                };
                redis.ConnectionFailed += (sender, args) =>
                {
                    Log.Error($"Redis connection failed: {args.EndPoint}, {args.FailureType}, {args.Exception?.Message}");
                };
                redis.ErrorMessage += (sender, args) =>
                {
                    Log.Error($"Redis error message: {args.Message}");
                };
                services.AddSingleton<IConnectionMultiplexer>(redis);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
            }
        }
    }
}