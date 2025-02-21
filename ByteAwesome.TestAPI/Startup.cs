using ByteAwesome.TestAPI.DbContexts;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ByteAwesome.StartupConfig;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ByteAwesome.TestAPI
{
    public partial class Startup
    {
        private const string _defaultCorsPolicyName = "AllowAny";
        private const string _microServiceApiName = "TestAPI";
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
            services.AddDbContextPool<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
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
            services.RegisterSwagger(ApiVersions, _microServiceApiName);
            PreInitialize(services);
            //Validare Required Field For All API Methods
            services.AddMvc(options => options.Filters.Add(new ValidateModelAttribute()));
        }
        public void Configure(IApplicationBuilder app)
        {
            IHttpContextAccessor httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
            CurrentSession.Configure(httpContextAccessor);
            LanguageService.Configure();

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
                var actionResult = ContextResponseHelper.CreateInternalServerErrorResponse(exception.Message);
                await context.SetResponseToHttpContext(actionResult);
            }));
            //register middleware
            app.UseMiddleware<DeviceInfoMiddleware>();
            app.UseMiddleware<LocationInfoMiddleware>();
            app.UseMiddleware<RequestLogContextMiddleware>();
            app.UseMiddleware<LanguageMiddleware>();
            app.UseMiddleware<UserContextMiddleware>();

            StartUpHelper.RegisterEndpoints(ApiVersions, _microServiceApiName, app, RegisterGRPCendpoint);
        }
        protected void RegisterGRPCendpoint(IEndpointRouteBuilder endpoints)
        { }
    }
}


