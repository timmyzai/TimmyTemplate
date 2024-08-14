using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Sinks.OpenTelemetry;

namespace ByteAwesome.StartupConfig
{
    public partial class StartUpHelper
    {
        public static void ConfigureSerilog(HostBuilderContext context, IServiceProvider service, LoggerConfiguration configuration)
        {
            var phoneRegex = new Regex(@"\+(\d{1,3})(?:[ -]?\d){6,14}\d", RegexOptions.Compiled);
            var customOperators = new List<IMaskingOperator>
            {
                new RegexMaskingOperator(phoneRegex)
            };
            configuration
            .Enrich.FromLogContext()
                .Enrich.FromLogContext()
                .Enrich.WithMemoryUsage()
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.Mode = MaskingMode.Globally;
                    options.MaskingOperators.AddRange(customOperators);
                })
                .Enrich.WithClientIp()
                .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(service)
                .WriteTo.Console()
                .WriteTo.OpenTelemetry(x =>
                    {
                        x.Endpoint = "http://localhost:5341/ingest/otlp/v1/logs";
                        x.Protocol = OtlpProtocol.HttpProtobuf;
                        x.Headers = new Dictionary<string, string>
                        {
                            { "X-Seq-ApiKey", "El8YicnRjkyN87PoTzXm" }
                        };
                    }
                );

        }
        public class RegexMaskingOperator : IMaskingOperator
        {
            private readonly Regex _regex;
            private readonly string _mask;

            public RegexMaskingOperator(Regex regex, string mask = "***MASKED***")
            {
                _regex = regex ?? throw new ArgumentNullException(nameof(regex));
                _mask = mask;
            }

            public MaskingResult Mask(string input, string mask)
            {
                if (input is null)
                {
                    throw new ArgumentNullException(nameof(input));
                }

                var match = _regex.IsMatch(input);
                if (!match)
                {
                    return MaskingResult.NoMatch;
                }

                var result = _regex.Replace(input, _mask);
                return new MaskingResult
                {
                    Match = true,
                    Result = result
                };
            }
        }
    }
}