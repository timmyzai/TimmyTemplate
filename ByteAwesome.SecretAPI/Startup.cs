using ByteAwesome.SecretAPI.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using grpcServer.Services;
using Serilog;
using ByteAwesome.StartupConfig;

namespace ByteAwesome.SecretAPI
{
    public partial class Startup
    {
        private const string _defaultCorsPolicyName = "AllowAny";
        private const string _microServiceApiName = "SecretAPI";
        private readonly IConfiguration configuration;
        private readonly List<string> ApiVersions;
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            ApiVersions = GeneralHelper.DiscoverApiVersions<Startup>(_microServiceApiName);
        }
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            var CorsOrigins = configuration.GetSection("CorsOrigins").Value;
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(CorsOrigins
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.TrimEnd('/'))
                            .ToArray())
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
            ));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateTokenIntegrityAttribute>();
                options.Conventions.Add(new NormalizeControllerNameConvention());
                options.Conventions.Add(new HttpMethodConvention());
            });
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddGrpc();
            StartUpHelper.ConfigureSwagger(ApiVersions, _microServiceApiName, services);
            PreInitialize(services);
            //Validare Required Field For All API Methods
            services.AddMvc(options => options.Filters.Add(new ValidateModelAttribute()));
        }
        public void Configure(IApplicationBuilder app)
        {
            IHttpContextAccessor httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
            CurrentSession.Configure(httpContextAccessor);
            LanguageService.Configure(httpContextAccessor);

            app.UseHttpsRedirection();
            app.UseApiVersioning();
            app.UseRouting();
            app.UseSerilogRequestLogging();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(_defaultCorsPolicyName);
            app.UseExceptionHandler(c => c.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                await ContextResponseHelper.SetInternalServerErrorResponse(context, exception.Message);
            }));
            //register middleware
            app.UseMiddleware<DeviceInfoMiddleware>();
            app.UseMiddleware<LocationInfoMiddleware>();
            app.UseMiddleware<RequestLogContextMiddleware>();
            app.UseMiddleware<LanguageMiddleware>();

            StartUpHelper.RegisterEndpoints(ApiVersions, _microServiceApiName, app, RegisterGRPCendpoint);
        }
        protected void RegisterGRPCendpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<TfaGrpcServer>();
            endpoints.MapGrpcService<OtpGrpcServer>();
            endpoints.MapGrpcService<AccessTokenGrpcServer>();
            endpoints.MapGrpcService<KmsKeysGrpcServer>();
            endpoints.MapGrpcService<PasskeyGrpcServer>();
        }
    }
}


