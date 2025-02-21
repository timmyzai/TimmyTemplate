using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ByteAwesome.StartupConfig
{
    public partial class StartUpHelper
    {
        public static void RegisterEndpoints(List<string> apiVersions, string microServiceApiName, IApplicationBuilder app, Action<IEndpointRouteBuilder> registerGrpcAction, Action<IEndpointRouteBuilder> registerSignalRAction = null)
        {
            var isProduction = GeneralHelper.IsProductionEnvironment();
            if (!isProduction)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    foreach (var version in apiVersions)
                    {
                        c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"ByteAwesome.{microServiceApiName} {version}");
                    }
                    c.RoutePrefix = "swagger";
                    c.DocumentTitle = $"ByteAwesome.{microServiceApiName} - Swagger UI";
                    c.DefaultModelsExpandDepth(-1);
                    c.ConfigObject.AdditionalItems["authorization"] = new Dictionary<string, object>
                    {
                        { "enabled", true },
                        { "name", "Authorization" },
                        { "value", "Bearer {JWT token}" }
                    };
                    c.OAuthUsePkce();
                    c.DisplayRequestDuration();
                    c.DocExpansion(DocExpansion.None);
                });

                app.UseDeveloperExceptionPage();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                registerGrpcAction(endpoints);
                if (registerSignalRAction is not null)
                {
                    registerSignalRAction(endpoints);
                }

                endpoints.MapGet("/", context =>
                {
                    if (!isProduction)
                    {
                        context.Response.Redirect("/swagger");
                    }
                    else
                    {
                        var problemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            Title = "Restricted access",
                            Detail = "You do not have permission to access this resource.",
                        };
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return context.Response.WriteAsJsonAsync(problemDetails);
                    }
                    return Task.CompletedTask;
                });
            });
        }
    }
}
