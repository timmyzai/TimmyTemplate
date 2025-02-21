using ByteAwesome.Services;
using ByteAwesome.TestAPI.Modules;
using ByteAwesome.StartupConfig;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ByteAwesome.TestAPI
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
        }
        protected void RegisterHttpClient(IServiceCollection services)
        {
        }
    }
}