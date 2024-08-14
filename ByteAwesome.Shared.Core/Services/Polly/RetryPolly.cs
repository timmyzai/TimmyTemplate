using Polly;
using Polly.Retry;
using RedLockNet;
using Serilog;

namespace ByteAwesome.Services
{
    public static class PolicyBuilder
    {
        public static AsyncRetryPolicy CreateRetry<T>(int retryCount = 3) where T : Exception
        {
            return Policy.Handle<T>()
                        .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) + ThreadSafeRandom.NextDouble()),
                            onRetry: (exception, timeSpan, retryCount, context) =>
                            {
                                Log.Warning($"Retry {retryCount} for {context.PolicyKey}: {exception.Message}. Waiting {timeSpan} before next retry.");
                            });
        }
        public static AsyncRetryPolicy<IRedLock> CreateRedisLockRetryPolicy(int retryCount = 3)
        {
            return Policy.HandleResult<IRedLock>(lockResult => !lockResult.IsAcquired)
                         .WaitAndRetryAsync(
                             retryCount,
                             retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                             onRetry: (outcome, timespan, retryAttempt, context) =>
                             {
                                 Log.Warning($"Attempt {retryAttempt}: Failed to acquire Redis lock. Retrying in {timespan.TotalSeconds} seconds.");
                             });
        }
    }
    public static class ThreadSafeRandom
    {
        private static readonly Random Global = new();
        [ThreadStatic]
        private static Random _local;

        public static double NextDouble()
        {
            Random inst = _local;
            if (inst is null)
            {
                int seed;
                lock (Global) seed = Global.Next();
                _local = inst = new Random(seed);
            }
            return inst.NextDouble();
        }
    }
}