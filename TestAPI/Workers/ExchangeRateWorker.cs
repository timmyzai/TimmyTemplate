using TestAPI.Helper.Services;

using AwesomeProject;

namespace TestAPI.Workers;

public class ExchangeRateWorker : BaseBackgroundWorker
{
    
    public ExchangeRateWorker(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        Interval = TimeSpan.FromMinutes(5);
        RunOnStart = true;
    }

    protected override async Task ProcessTaskAsync(CancellationToken stoppingToken)
    {
        using var scope = ScopeFactory.CreateScope();
        var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
        
        await exchangeRateService.StoreExchangeRates();
    }
}