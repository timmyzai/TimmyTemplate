using SecretAPI.Repository;
using SecretAPI.Modules;
using SecretAPI.Helper;
using Services;
using StartupConfig;
using Newtonsoft.Json;

using AwesomeProject;

namespace SecretAPI
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

            //Modules
            services.Configure<AppModuleConfig>(configuration.GetSection("App"));

            //Services
            services.AddScoped<IJwtManagement, JwtManagement>();

            //Singleton Services
            services.AddSingleton<IRedisCacheService, RedisCacheService>();

            //External Apis

            //Redis
            var redisConnectionString = configuration.GetConnectionString("Redis");
            RedisConnectionManager.ConnectRedis(services, redisConnectionString);

            //Special UserIds
            var specialUserIds = new HashSet<string>();
            if (GeneralHelper.IsDevelopmentEnvironment())
            {
                specialUserIds = configuration.GetSection("SpecialUserIds").Get<HashSet<string>>();
            }
            else
            {
                var specialUserJsonValue = configuration.GetSection("SpecialUserIds").Value;
                specialUserIds = JsonConvert.DeserializeObject<HashSet<string>>(specialUserJsonValue);
            }
            services.AddSingleton(new SpecialUserModuleConfig
            {
                SpecialUserIds = GeneralHelper.IsProductionEnvironment() ? [] : specialUserIds
            });
        }
        protected void PreLoadServices(IServiceCollection services)
        {
            services.AddSingleton(MappingConfig.RegisterMaps().CreateMapper());
            services.AddSingleton(StartUpHelper.ConfigureAuthentication(configuration, services));
        }
    }
}