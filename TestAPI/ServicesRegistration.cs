using Services;
using TestAPI.Modules;
using StartupConfig;
using TestAPI.Helper.Services;
using TestAPI.Repositories;
using TestAPI.Workers;
using AwesomeProject;

namespace TestAPI
{
    public partial class Startup
    {
        private void PreInitialize(IServiceCollection services)
        {
            //Load Startup Services
            PreLoadServices(services);

            //Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
            services.AddScoped<IBaseCurrencyRepository, BaseCurrencyRepository>();
            services.AddScoped<IExchangeRateService, ExchangeRateService>();
            
            //Singleton Services
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<IRedisLockManager, RedisLockManager>();

            //Modules
            services.Configure<AppModuleConfig>(configuration.GetSection("App"));
            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));

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
            services.AddHostedService<ExchangeRateWorker>(); 
        }
        protected void RegisterHttpClient(IServiceCollection services)
        {
            services.AddHttpClient<IExchangeRateService, ExchangeRateService>();
        }
    }
}