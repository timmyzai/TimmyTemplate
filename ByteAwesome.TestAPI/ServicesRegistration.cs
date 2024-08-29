using ByteAwesome.Services;
using ByteAwesome.TestAPI.Services;
using ByteAwesome.TestAPI.Modules;
using ByteAwesome.StartupConfig;

namespace ByteAwesome.TestAPI
{
    public partial class Startup
    {
        private void PreInitialize(IServiceCollection services)
        {
            //Load Startup Services
            PreLoadServices(services);

            //Services
            services.AddScoped<ITimerService, TimerService>();
            services.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();
            services.AddScoped<ICurrentUnitOfWork, CurrentUnitOfWork>();
            services.AddScoped<ICacheService, CacheService>();

            //Singleton Services
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<IRedisLockManager, RedisLockManager>();

            //Modules
            services.Configure<AppModuleConfig>(configuration.GetSection("App"));

            //HttpClient
            RegisterHttpClient(services);
            //GRPC
            RegisterGRPC(services);
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
            try
            {
                services.AddSingleton<IGeoIPService, GeoIPService>(provider =>
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "GeoIpService", "GeoLite2-City.mmdb");
                    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                    return new GeoIPService(path, httpContextAccessor);
                });
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
            }
        }
        protected void RegisterWorkers(IServiceCollection services)
        {
        }
        protected void RegisterGRPC(IServiceCollection services)
        {
            // services.AddScoped<IWalletGrpcClient, WalletGrpcClient>();

            // var walletGrpcUrl = configuration["GRPC:WalletURL"];
            // AddGrpcClient<WalletGrpcService.WalletGrpcServiceClient>(walletGrpcUrl);

            // void AddGrpcClient<TClient>(string url) where TClient : class
            // {
            //     services.AddGrpcClient<TClient>(o => o.Address = new Uri(url))
            //     .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            //     {
            //         ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            //     });
            // }
        }
        protected void RegisterHttpClient(IServiceCollection services)
        {
        }
    }
}