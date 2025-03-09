using Services;
using TestAPI.Modules;
using StartupConfig;
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

            //Singleton Services
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<IRedisLockManager, RedisLockManager>();

            //Modules
            services.Configure<AppModuleConfig>(configuration.GetSection("App"));

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
        }
    }
}