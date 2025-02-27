using ByteAwesome.TestAPI.Helper.Services;

namespace ByteAwesome.TestAPI.Workers;

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

        try
        {
            await exchangeRateService.StoreExchangeRates();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}