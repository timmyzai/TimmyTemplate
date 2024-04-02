using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

namespace ByteAwesome.Services.TestAPI
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
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(
                        options =>
                        {
                            options.ListenLocalhost(7182, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1;
                                // listenOptions.UseHttps();//when local need to disable
                            });
                        });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
