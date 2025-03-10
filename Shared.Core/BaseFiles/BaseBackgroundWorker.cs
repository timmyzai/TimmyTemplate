using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AwesomeProject
{
    public abstract class BaseBackgroundWorker : BackgroundService
    {
        protected readonly IServiceScopeFactory ScopeFactory;
        protected TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1); // Default interval
        protected DateTime? StartAt { get; set; }
        protected bool RunOnStart { get; set; } = false;

        protected BaseBackgroundWorker(IServiceScopeFactory scopeFactory)
        {
            ScopeFactory = scopeFactory;
        }

        protected abstract Task ProcessTaskAsync(CancellationToken stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimeSpan initialDelay = CalculateInitialDelay();
            if (initialDelay > TimeSpan.Zero)
            {
                Log.Information("{WorkerName} initial delay set to {DelaySeconds} seconds.", GetType().Name, initialDelay.TotalSeconds);
                await Task.Delay(initialDelay, stoppingToken);
                await ExecuteTaskAsync(stoppingToken);
            }
            else if (RunOnStart)
            {
                Log.Information("{WorkerName} executing immediately.", GetType().Name);
                await ExecuteTaskAsync(stoppingToken);
            }

            using PeriodicTimer timer = new(Interval);
            Log.Information("{WorkerName} interval set to {IntervalSeconds} seconds.", GetType().Name, Interval.TotalSeconds);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ExecuteTaskAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Information("{WorkerName} is stopping.", GetType().Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{WorkerName} encountered an error.", GetType().Name);
                throw;
            }
        }

        private TimeSpan CalculateInitialDelay()
        {
            if (StartAt.HasValue)
            {
                var delay = StartAt.Value - DateTime.UtcNow;
                return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
            }
            return RunOnStart ? TimeSpan.Zero : Interval;
        }

        private async Task ExecuteTaskAsync(CancellationToken stoppingToken)
        {
            Log.Information("{WorkerName} is starting a new task cycle.", GetType().Name);
            try
            {
                await ProcessTaskAsync(stoppingToken);
                Log.Information("{WorkerName} has completed a task cycle successfully.", GetType().Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{WorkerName} encountered an error during task execution.", GetType().Name);
            }
            finally
            {
                Log.Information("{WorkerName} is ready for the next cycle.", GetType().Name);
            }
        }

        public override void Dispose()
        {
            Log.Information("{WorkerName} is being disposed.", GetType().Name);
            base.Dispose();
        }
    }
}
