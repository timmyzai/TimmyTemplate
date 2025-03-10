using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

namespace StartupConfig
{
    public partial class StartUpHelper
    {
        public static void ConfigureSerilog(HostBuilderContext context, IServiceProvider service, LoggerConfiguration configuration)
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMemoryUsage()
                .Enrich.WithClientIp()
                .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(service);
            ConfigureSinks(context, configuration);
        }
        private static void ConfigureSinks(HostBuilderContext context, LoggerConfiguration configuration)
        {
            var config = context.Configuration;
            var useSeq = config.GetValue<bool>("Seq:IsEnabled");
            var useLog = config.GetValue<bool>("Seq:IsEnabledLog");
            if (useSeq)
            {
                var seqEndpoint = config.GetValue<string>("Seq:Endpoint");
                var seqApiKey = config.GetValue<string>("Seq:ApiKey");
                configuration.WriteTo.OpenTelemetry(x =>
                {
                    x.Endpoint = seqEndpoint;
                    x.Protocol = OtlpProtocol.HttpProtobuf;
                    x.Headers = new Dictionary<string, string> { { "X-Seq-ApiKey", seqApiKey } };
                });
            }
            if (useLog)
            {
                configuration.WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Hour,
                    fileSizeLimitBytes: 10485760, // 10MB
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3} {Username} {Message:lj}{Exception}{NewLine}"
                );
            }
            if (context.HostingEnvironment.IsDevelopment())
            {
                configuration.WriteTo.Console();
            }
        }
    }
}