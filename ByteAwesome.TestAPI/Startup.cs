using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ByteAwesome.TestAPI.DbContexts;

namespace ByteAwesome.TestAPI
{
    public partial class Startup
    {
        private const string _defaultCorsPolicyName = "AllowAny";
        private const string _microServiceApiName = "Test Api";
        private readonly IConfiguration configuration;
        private readonly bool IsDevelopment = false;
        private readonly bool IsProduction = false;
        private readonly List<string> ApiVersions;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.configuration = configuration;
            IsDevelopment = environment.IsDevelopment();
            IsProduction = environment.IsProduction();
            ApiVersions = DiscoverApiVersions();
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
                options.Filters.Add(new ValidateTokenIntegrityAttribute());
                options.Conventions.Add(new NormalizeControllerNameConvention());
            });
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            ConfigureSwagger(services);
            PreInitialize(services);
            //Validare Required Field For All API Methods
            services.AddMvc(options => options.Filters.Add(new ValidateModelAttribute()));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Inject httpContextAccessor to user CurrentSession
            IHttpContextAccessor httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
            CurrentSession.Configure(httpContextAccessor);
            LanguageService.Configure(httpContextAccessor);
            //register app use session
            app.UseExceptionHandler(c => c.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                var errorResponse = new ResponseDto<object>
                {
                    IsSuccess = false,
                    DisplayMessage = "An error occurred while processing your request.",
                    Error = new ErrorDto
                    {
                        StatusCode = ErrorCodes.General.UnhandledError,
                        ErrorMessage = exception.Message
                    },
                    Result = null
                };
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(errorResponse);
            }));
            app.UseHttpsRedirection();
            app.UseApiVersioning();
            app.UseRouting();
            app.UseSerilogRequestLogging();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(_defaultCorsPolicyName);

            //register middleware
            app.UseMiddleware<DeviceInfoMiddleware>();
            app.UseMiddleware<LocationInfoMiddleware>();

            //register endpoint
            if (!IsProduction)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    {
                        foreach (var version in ApiVersions)
                        {
                            c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"ByteAwesome.{_microServiceApiName} {version}");
                        }
                        c.RoutePrefix = "swagger";

                        c.DocumentTitle = $"ByteAwesome.{_microServiceApiName} - Swagger UI";
                        c.DefaultModelsExpandDepth(-1);
                        c.ConfigObject.AdditionalItems["authorization"] = new
                        {
                            enabled = true,
                            name = "Authorization",
                            value = "Bearer {JWT token}"
                        };
                        c.OAuthUsePkce();
                        c.DisplayRequestDuration();
                        c.DocExpansion(DocExpansion.None);
                    });

                app.UseDeveloperExceptionPage();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapGet("/", context =>
                    {
                        context.Response.Redirect("/swagger");
                        return Task.CompletedTask;
                    });
                });
            }
            else
            {
                app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                        endpoints.MapGet("/", async context =>
                        {
                            var problemDetails = new ProblemDetails
                            {
                                Status = (int)StatusCodes.Status401Unauthorized,
                                Title = "Restricted access",
                                Detail = "You do not have permission to access this resource.",
                            };

                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsJsonAsync(problemDetails);
                        });
                    });
            }
        }
        protected List<string> DiscoverApiVersions()
        {
            var apiVersions = typeof(Startup).Assembly.GetTypes()
                .Where(type => type.Namespace != null && type.Namespace.Equals($"ByteAwesome.{_microServiceApiName}.Controllers"))
                .Where(type => type.GetCustomAttributes(typeof(ApiVersionAttribute), true).Any())
                .SelectMany(type => type.GetCustomAttributes(typeof(ApiVersionAttribute), true))
                .Cast<ApiVersionAttribute>()
                .SelectMany(attr => attr.Versions)
                .Distinct()
                .Select(v => $"v{v}")
                .ToList();
            return apiVersions;
        }
    }
}


