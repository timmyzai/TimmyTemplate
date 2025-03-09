using StartupConfig;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

using AwesomeProject;

namespace SecretAPI
{
    public static class Program
    {
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
                    try
                    {
                        if (context.HostingEnvironment.EnvironmentName != Environments.Development)
                        {
                            config.Add(new StartUpHelper.AwsSecretsConfig(config.Build()));
                        }
                    }
                    catch (Exception ex)
                    {
                        ActionResultHandler.HandleException(ex);
                        throw;
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

    }
}
