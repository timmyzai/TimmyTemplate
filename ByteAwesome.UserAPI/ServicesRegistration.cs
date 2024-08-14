using ByteAwesome.UserAPI.Repository;
using ByteAwesome.UserAPI.Modules;
using ByteAwesome.UserAPI.GrpcClient;
using ByteAwesome.UserAPI.Helper.UnitOfWork;
using ByteAwesome.UserAPI.ExternalApis.SumsubKyc.Modules;
using ByteAwesome.UserAPI.ExternalApis.SumsubKycService;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.NotificationAPI.Grpc;
using ByteAwesome.WalletAPI.Grpc;
using Microsoft.Extensions.Options;
using ByteAwesome.Services;
using ByteAwesome.UserAPI.Services;
using ByteAwesome.StartupConfig;

namespace ByteAwesome.UserAPI
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
            services.AddScoped<IKycRepository, KycRepository>();
            services.AddScoped<IUserLoginSessionInfoRepository, UserLoginSessionInfoRepository>();

            //Modules
            var encryptSecretKey = configuration["App:EncryptSecretKey"];

            services.Configure<AppModuleConfig>(configuration.GetSection("App"));
            services.Configure<MailGunModuleConfig>(configuration.GetSection("MailGunConfig"));
            services.Configure<BrevoMailModuleConfig>(configuration.GetSection("BrevoConfig"));
            services.Configure<GMailModuleConfig>(configuration.GetSection("GMailConfig"));

            services.Configure<SumsubKycModuleConfig>(SumsubConfig =>
            {
                SumsubConfig.BaseUrl = configuration["SumsubKyc:BaseUrl"];
                SumsubConfig.AppToken = AesEncoder.DecryptString(configuration["SumsubKyc:AppToken"], encryptSecretKey);
                SumsubConfig.SecretKey = AesEncoder.DecryptString(configuration["SumsubKyc:SecretKey"], encryptSecretKey);
            });

            //Services
            services.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();
            services.AddScoped<ICurrentUnitOfWork, CurrentUnitOfWork>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IRedisCacheService, RedisCacheService>(); services.AddScoped<IUserServices, UserServices>();

            //External Apis
            services.AddScoped<ISumsubKycService, SumsubKycService>();

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
            services.AddScoped<IOtpGrpcClient, OtpGrpcClient>();
            services.AddScoped<ITfaGrpcClient, TfaGrpcClient>();
            services.AddScoped<IAccessTokenGrpcClient, AccessTokenGrpcClient>();
            services.AddScoped<INotisGrpcClient, NotisGrpcClient>();
            services.AddScoped<IKmsKeysGrpcClient, KmsKeysGrpcClient>();
            services.AddScoped<IWalletGrpcClient, WalletGrpcClient>();
            services.AddScoped<IPasskeyGrpcClient, PasskeyGrpcClient>();

            string secretGrpcUrl = configuration["GRPC:SecretURL"];
            AddGrpcClient<OtpGrpcService.OtpGrpcServiceClient>(secretGrpcUrl);
            AddGrpcClient<TfaGrpcService.TfaGrpcServiceClient>(secretGrpcUrl);
            AddGrpcClient<AccessTokenGrpcService.AccessTokenGrpcServiceClient>(secretGrpcUrl);
            AddGrpcClient<KmsKeysGrpcService.KmsKeysGrpcServiceClient>(secretGrpcUrl);
            AddGrpcClient<PasskeyGrpcService.PasskeyGrpcServiceClient>(secretGrpcUrl);

            string notisGrpcUrl = configuration["GRPC:NotisURL"];
            AddGrpcClient<NotisGrpcService.NotisGrpcServiceClient>(notisGrpcUrl);

            var walletGrpcUrl = configuration["GRPC:WalletURL"];
            AddGrpcClient<WalletGrpcService.WalletGrpcServiceClient>(walletGrpcUrl);

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
            PollyPolicies.AddHttpClient<SumsubKycService>(services,
                (serviceProvider, client) =>
                {
                    var config = serviceProvider.GetRequiredService<IOptions<SumsubKycModuleConfig>>();
                    client.BaseAddress = new Uri(config.Value.BaseUrl);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });
        }
    }
}