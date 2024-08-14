using ByteAwesome.SecretAPI.Repository;
using ByteAwesome.SecretAPI.Modules;
using ByteAwesome.WalletAPI.ExternalApis.AwsKmsServices;
using ByteAwesome.SecretAPI.Helper;
using ByteAwesome.Services;
using ByteAwesome.StartupConfig;

namespace ByteAwesome.SecretAPI
{
    public partial class Startup
    {
        private void PreInitialize(IServiceCollection services)
        {
            //Load Startup Services
            PreLoadServices(services);

            //Repositories
            services.AddScoped<ITwoFactorAuthRepository, TwoFactorAuthRepository>();
            services.AddScoped<IOtpRepository, OtpRepository>();
            services.AddScoped<IKmsKeysRepository, KmsKeysRepository>();
            services.AddScoped<IPasskeyRepository, PasskeyRepository>();

            //Modules
            services.Configure<AppModuleConfig>(configuration.GetSection("App"));
            var fido2Config = configuration.GetSection("Fido2").Get<Fido2ModuleConfig>();
            services.Configure<AwsModuleConfig>(configuration.GetSection("AWS:Config"));

            //Services
            services.AddScoped<IJwtManagement, JwtManagement>();
            services.AddScoped<ICacheService, CacheService>();

            //Singleton Services
            services.AddSingleton<IRedisCacheService, RedisCacheService>();

            //External Apis
            services.AddScoped<IAwsKmsServices, AwsKmsServices>();

            //HttpClient
            RegisterHttpClient(services);
            //GRPC
            RegisterGRPC(services);
            //Redis
            var redisConnectionString = configuration.GetConnectionString("Redis");
            RedisConnectionManager.ConnectRedis(services, redisConnectionString);

            //Fido2
            services.AddFido2(options =>
            {
                options.ServerDomain = fido2Config.ServerDomain;
                options.ServerName = fido2Config.ServerName;
                options.Origins = new HashSet<string>(fido2Config.Origins);
                options.TimestampDriftTolerance = fido2Config.TimestampDriftTolerance;
            });
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
                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "GeoIpService", "GeoLite2-City.mmdb");
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
        }
        protected void RegisterHttpClient(IServiceCollection services)
        {
        }
    }
}