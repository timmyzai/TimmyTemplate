using ByteAwesome.Services.TestAPI.DbContexts;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using ByteAwesome.Services.TestAPI.Helper;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.Services.TestAPI
{
    public partial class Startup
    {
        private const string _defaultCorsPolicyName = "AllowAny";
        private const string _defaultApiVersion = "v1";
        private const string _microServiceApiName = "TestAPI";
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
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
            services.AddControllers();
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

            //register endpoint
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint($"/swagger/{_defaultApiVersion}/swagger.json", $"ByteAwesome.Services.{_microServiceApiName} {_defaultApiVersion}");
                        c.RoutePrefix = "swagger";

                        c.DocumentTitle = $"{_microServiceApiName} - Swagger UI";
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
    }
}


