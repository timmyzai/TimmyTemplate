using Services;
using TestAPI.Modules;
using StartupConfig;
using AwesomeProject;
using TestAPI.Repository;
using TestAPI.ExternalApi.CoinGeckoApi;
using TestAPI.ExternalApi;
using TestAPI.Service;
using Microsoft.Extensions.Options;

namespace TestAPI
{
    public partial class Startup
    {
        private void PreInitialize(IServiceCollection services)
        {
            //Load Startup Services
            PreLoadServices(services);

            //Repositories
            services.AddScoped<ICryptoOrderRepository, CryptoOrderRepository>();

            services.AddScoped<ICoinGeckoService, CoinGeckoService>();

            services.AddScoped<IOrderService, OrderService>();

            //Singleton Services
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<IRedisLockManager, RedisLockManager>();

            //Modules
            services.Configure<AppModuleConfig>(configuration.GetSection("App"));
            services.Configure<CoinGeckoConfig>(configuration.GetSection("CoinGecko"));

            //HttpClient
            RegisterHttpClient(services);
            
            //Redis
            var redisConnectionString = configuration.GetConnectionString("Redis");
            RedisConnectionManager.ConnectRedis(services, redisConnectionString);
            //Worker
            if (!GeneralHelper.IsDevelopmentEnvironment())
            {
                RegisterWorkers(services);
            }
        }
        protected void PreLoadServices(IServiceCollection services)
        {
            services.AddSingleton(MappingConfig.RegisterMaps().CreateMapper());
            services.AddSingleton(StartUpHelper.ConfigureAuthentication(configuration, services));
        }
        protected void RegisterWorkers(IServiceCollection services)
        {
        }
        protected void RegisterHttpClient(IServiceCollection services)
        {
            services.AddHttpClient<ICoinGeckoService, CoinGeckoService>((provider, client) =>
            {
                var config = provider.GetRequiredService<IOptions<CoinGeckoConfig>>().Value;
                client.BaseAddress = new Uri("https://api.coingecko.com");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("x-cg-demo-api-key", config.ApiKey);
            });
        }
    }
}