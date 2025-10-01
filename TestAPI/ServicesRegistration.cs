using Services;
using TestAPI.Modules;
using StartupConfig;
using AwesomeProject;
using TestAPI.Repository;
using UnitOfWork;
using TestAPI.Services;
using Microsoft.Extensions.Options;
using TestAPI.Repository.ScoreRuleRepository;

namespace TestAPI
{
    public partial class Startup
    {
        private void PreInitialize(IServiceCollection services)
        {
            //Load Startup Services
            PreLoadServices(services);

            //Repositories
            services.AddScoped<GameRepository>();
            services.AddScoped<GameScoreRepository>();
            services.AddScoped<ScoreRuleRepository>();

            //Services
            services.AddScoped<GameService>();
            services.AddScoped<GameScoreServices>();

            //Singleton Services
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<IRedisLockManager, RedisLockManager>();

            //Modules
            services.Configure<AppModuleConfig>(configuration.GetSection("App"));
            services.Configure<DictionaryApiModuleConfig>(dac =>
            {
                dac.BaseUrl = configuration["DictionaryApi:BaseUrl"];
            });
            services.Configure<WordListConfig>(dac =>
            {
                dac.Path = configuration["WordList:Path"];
            });
            services.Configure<List<ScoreRule>>(configuration.GetSection("ScoreRules"));

            services.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();

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
            PollyPolicies.AddHttpClient<DictionaryApiClient>(services,
                           (serviceProvider, client) =>
                           {
                               var config = serviceProvider.GetRequiredService<IOptions<DictionaryApiModuleConfig>>();
                               client.BaseAddress = new Uri(config.Value.BaseUrl);
                               client.DefaultRequestHeaders.Add("Accept", "application/json");
                           });
        }
    }
}