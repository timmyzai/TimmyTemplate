using ByteAwesome.StartupConfig;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

namespace ByteAwesome.TestAPI
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
                .UseSerilog(StartUpHelper.ConfigureSerilog)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(
                        options =>
                        {
                            options.ListenLocalhost(7186, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1;
                            });
                        });
                    webBuilder.UseStartup<Startup>();
                });

    }
}
