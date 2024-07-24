using BenchmarkDotNet.Attributes;
using ByteAwesome.TestAPI.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ByteAwesome.TestAPI
{
    [Config(typeof(CustomConfig))]
    public class WalletApiBenchmark
    {
        private ServiceProvider serviceProvider;
        // private IWalletGroupsRepository walletGroupsRepository;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
            // walletGroupsRepository = serviceProvider.GetRequiredService<IWalletGroupsRepository>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddHttpContextAccessor();
            services.AddSingleton(MappingConfig.RegisterMaps().CreateMapper());
            // services.AddScoped<IWalletGroupsRepository, WalletGroupsRepository>();
        }

        // [Benchmark(Baseline = true)]
        // public async Task GetMyWalletGroups()
        // {
        //     try
        //     {
        //         await walletGroupsRepository.GetMyWalletGroups(Guid.Parse("08dbe983-add3-435b-86cf-6014f229ab27"));
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"An error occurred while executing the benchmark: {ex.Message}");
        //     }
        // }
        // [Benchmark]
        // public async Task GetMyWalletGroups_CompiledQuery()
        // {
        //     try
        //     {
        //         await walletGroupsRepository.GetMyWalletGroups_CompiledQuery(Guid.Parse("08dbe983-add3-435b-86cf-6014f229ab27"));
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"An error occurred while executing the benchmark: {ex.Message}");
        //     }
        // }

        [GlobalCleanup]
        public void Cleanup()
        {
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
