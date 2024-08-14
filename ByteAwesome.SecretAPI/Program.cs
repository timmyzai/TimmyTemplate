using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using ByteAwesome.StartupConfig;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json;
using Serilog;

namespace ByteAwesome.SecretAPI
{
    public static class Program
    {
        private static AmazonSecretsManagerClient awsClient;

        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
            Log.Information("Starting Up Server");
            try
            {
                CreateHostBuilder(args).Build().Run();
                Log.Information("Stopped Server Cleanly.");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var builtConfig = config.Build();
                    var environmentName = context.HostingEnvironment.EnvironmentName;
                    if(environmentName != Environments.Development)
                    {
                        LoadAwsSecrets(config, builtConfig["AWS:SecretManager:SecretId"], builtConfig["AWS:SecretManager:Region"]);
                    }
                })
                .UseSerilog(StartUpHelper.ConfigureSerilog)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(
                        options =>
                        {
                            options.ListenLocalhost(7183, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1;
                                // listenOptions.UseHttps();//when local need to disable
                            });
                            options.ListenLocalhost(5001, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http2;
                                var isDevelopment = webBuilder.GetSetting("environment") == Environments.Development;
                                if (!isDevelopment)
                                {
                                    listenOptions.UseHttps();
                                }
                            });
                        });
                    webBuilder.UseStartup<Startup>();
                });

        private static void LoadAwsSecrets(IConfigurationBuilder config, string secretId, string region)
        {            
            if (!string.IsNullOrEmpty(secretId))
            {
                if (awsClient is null)
                {
                    awsClient = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
                }
                
                var response = awsClient.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretId }).Result;
                var secretString = response.SecretString;
                if (secretString is not null)
                {
                    var secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(secretString);
                    config.AddInMemoryCollection(secrets);
                }
            }
        }
    
    }
}
