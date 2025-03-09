using UserAPI.Repository;
using UserAPI.Modules;
using UserAPI.GrpcClient;
using SecretAPI.Grpc;
using Services;
using UserAPI.Services;
using StartupConfig;
using UnitOfWork;
using AwesomeProject;

namespace UserAPI
{
    public partial class Startup
    {
        private void PreInitialize(IServiceCollection services)
        {
            //Load Startup Services
            PreLoadServices(services);

            //Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRolesRepository, UserRolesRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            //Modules
            var encryptSecretKey = configuration["App:EncryptSecretKey"];

            services.Configure<AppModuleConfig>(configuration.GetSection("App"));
            services.Configure<BrevoMailModuleConfig>(configuration.GetSection("BrevoConfig"));
            //Services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();
            services.AddScoped<IRedisCacheService, RedisCacheService>(); services.AddScoped<IUserServices, UserServices>();

            //External Apis

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
        }
        protected void RegisterWorkers(IServiceCollection services)
        {
        }
        protected void RegisterGRPC(IServiceCollection services)
        {
            services.AddScoped<IOtpGrpcClient, OtpGrpcClient>();
            services.AddScoped<ITfaGrpcClient, TfaGrpcClient>();
            services.AddScoped<IAccessTokenGrpcClient, AccessTokenGrpcClient>();

            string secretGrpcUrl = configuration["GRPC:SecretURL"];
            AddGrpcClient<OtpGrpcService.OtpGrpcServiceClient>(secretGrpcUrl);
            AddGrpcClient<TfaGrpcService.TfaGrpcServiceClient>(secretGrpcUrl);
            AddGrpcClient<AccessTokenGrpcService.AccessTokenGrpcServiceClient>(secretGrpcUrl);

            void AddGrpcClient<TClient>(string url) where TClient : class
            {
                services.AddGrpcClient<TClient>(o => o.Address = new Uri(url))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            }
        }
        protected void RegisterHttpClient(IServiceCollection services)
        {
        }
    }
}