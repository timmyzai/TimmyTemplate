using ByteAwesome.TestAPI.Modules;
using ByteAwesome.Services;
using Serilog;
using StackExchange.Redis;

namespace ByteAwesome.TestAPI
{
    public partial class Startup
    {
        private void PreInitialize(IServiceCollection services)
        {
            //Load Startup Services
            PreLoadServices(services);

            services.Configure<AppModuleConfig>(configuration.GetSection("App"));

            //HttpClient
            RegisterHttpClient(services);
            //GRPC
            RegisterGRPC(services);
            //Redis
            ConnectRedis(services);
            //Worker
            RegisterWorkers(services);
        }
        protected void PreLoadServices(IServiceCollection services)
        {
            services.AddSingleton(MappingConfig.RegisterMaps().CreateMapper());
            services.AddSingleton(ConfigureAuthentication(services));
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
                Log.Error(ex, $"Load GeoIP_Databases - Error loading startup data: {ex.Message}");
            }
        }
        protected void ConnectRedis(IServiceCollection services)
        {
            try
            {
                var redisConnectionString = configuration.GetConnectionString("Redis");
                var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                services.AddSingleton<IConnectionMultiplexer>(redis);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ErrorCodes.General.RedisConnection} - Error connecting to Redis: {ex.Message}");
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