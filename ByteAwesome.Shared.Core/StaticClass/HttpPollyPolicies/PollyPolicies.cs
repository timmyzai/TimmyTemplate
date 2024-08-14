using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace ByteAwesome
{
    public static class PollyPolicies
    {
        public static void AddHttpClient<TClient>(
            IServiceCollection services,
            Action<IServiceProvider, HttpClient> configureClient,
            HttpClientPolicySettingsDto settings = null)
            where TClient : class
        {
            settings ??= new HttpClientPolicySettingsDto();
            services.AddHttpClient<TClient>()
                    .ConfigureHttpClient(configureClient)
                    .AddPolicyHandler(GetRetryPolicy(settings.RetryCount))
                    .AddPolicyHandler(GetCircuitBreakerPolicy(settings.EventsAllowedBeforeBreak, settings.DurationOfBreakInSeconds))
                    .AddPolicyHandler(GetTimeoutPolicy(settings.TimeoutInSeconds))
                    .AddPolicyHandler(GetFallbackPolicy());
        }
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                        TimeSpan.FromMilliseconds(new Random().Next(0, 100)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Log.Warning("Retry attempt {RetryAttempt} after {Timespan} seconds due to {ExceptionMessage}", retryAttempt, timespan.TotalSeconds, outcome.Exception?.Message);
                    });
        }
        // This method creates a Circuit Breaker policy. It allows a certain number of events before breaking the circuit for a specified duration.
        // When the circuit is broken, it logs a warning message. When the circuit is reset or half-open, it logs an informational message.
        // Closed State: Normal operation, requests are allowed.
        // Open State: Requests are rejected immediately to prevent overloading the failing service.
        // Half-Open State: Allows a few requests to test if the service has recovered.
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int eventsAllowedBeforeBreak, int durationOfBreakInSeconds)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: eventsAllowedBeforeBreak,
                    durationOfBreak: TimeSpan.FromSeconds(durationOfBreakInSeconds),
                    onBreak: (outcome, timespan) =>
                    {
                        Log.Warning("Circuit broken for {Timespan} seconds due to {ExceptionMessage}", timespan.TotalSeconds, outcome.Exception?.Message);
                    },
                    onReset: () =>
                    {
                        Log.Information("Circuit reset");
                    },
                    onHalfOpen: () =>
                    {
                        Log.Information("Circuit half-open");
                    });
        }
        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutInSeconds)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(timeoutInSeconds, TimeoutStrategy.Optimistic, onTimeoutAsync: (context, timespan, task) =>
            {
                Log.Warning("Timeout after {Timespan} seconds", timespan.TotalSeconds);
                return Task.CompletedTask;
            });
        }
        private static IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
        {
            return Policy<HttpResponseMessage>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackValue: new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("Fallback response")
                    },
                    onFallbackAsync: (outcome, context) =>
                    {
                        Log.Warning("Fallback due to {ExceptionMessage}", outcome.Exception?.Message);
                        return Task.CompletedTask;
                    });
        }
    }
}
