using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using AwesomeProject;

namespace StartupConfig
{
    public static class SwaggerConfig
    {
        public static void RegisterSwagger(this IServiceCollection services, List<string> apiVersions, string microServiceApiName)
        {
            services.AddSwaggerGen(options =>
            {
                // Create Swagger documents for each API version
                foreach (var version in apiVersions)
                {
                    options.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = $"{microServiceApiName}",
                        Version = version
                    });
                }
                options.UseInlineDefinitionsForEnums();
                // Enable support for adding annotations to API operations
                options.EnableAnnotations();

                // Define the security scheme for Bearer tokens
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter 'Bearer' followed by your token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Apply security requirements globally
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                // Apply custom filters
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPath>();
                options.DocInclusionPredicate((version, desc) => IsApiVersionValid(version, desc));
                options.OperationFilter<RemoveVersionFromParameter>();
                options.OperationFilter<AddAcceptLanguageHeaderParameter>();
                options.OperationFilter<InjectFromHeaderOperationFilter>();
            });
        }

        private static bool IsApiVersionValid(string version, ApiDescription desc)
        {
            // Check if the method info has a valid API version
            if (!desc.TryGetMethodInfo(out MethodInfo methodInfo))
                return false;

            var versions = methodInfo.DeclaringType?.GetCustomAttributes<ApiVersionAttribute>(true).SelectMany(attr => attr.Versions) ?? [];

            // Return true if the version matches or if no version is specified, default to v1.0
            return versions.Any(v => $"v{v}" == version) || (versions.Count() == 0 && version == "v1.0");
        }
        private class SwaggerEnumSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                var type = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
                if (type.IsEnum)
                {
                    schema.Type = "string";
                    schema.Enum = Enum.GetNames(type)
                                      .Select(n => new OpenApiString(n))
                                      .ToList<IOpenApiAny>();

                    // Add a null option for nullable enums
                    if (Nullable.GetUnderlyingType(context.Type) != null)
                    {
                        schema.Nullable = true;
                    }
                }
            }
        }
        // Operation filter to add Accept-Language header parameter
        private class AddAcceptLanguageHeaderParameter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Accept-Language",
                    In = ParameterLocation.Header,
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string" },
                    Description = "Language code"
                });
            }
        }

        // Operation filter to remove version parameter from the operation
        private class RemoveVersionFromParameter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");
                if (versionParameter is not null)
                {
                    operation.Parameters.Remove(versionParameter);
                }
            }
        }

        // Document filter to replace version placeholder in paths
        private class ReplaceVersionWithExactValueInPath : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                var updatedPaths = new OpenApiPaths();
                foreach (var path in swaggerDoc.Paths)
                {
                    updatedPaths.Add(path.Key.Replace("v{version}", swaggerDoc.Info.Version), path.Value);
                }
                swaggerDoc.Paths = updatedPaths;
            }
        }

        // Operation filter to inject custom headers from attributes
        private class InjectFromHeaderOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var injectHeaderAttributes = context.MethodInfo.GetCustomAttributes<InjectFromHeaderAttribute>(true);
                foreach (var attr in injectHeaderAttributes)
                {
                    foreach (var header in attr.headers)
                    {
                        var existingParam = operation.Parameters.FirstOrDefault(p => p.Name == header.Name);
                        if (existingParam is not null)
                        {
                            existingParam.Required |= header.IsRequired;
                        }
                        else
                        {
                            operation.Parameters.Add(new OpenApiParameter
                            {
                                Name = header.Name,
                                In = ParameterLocation.Header,
                                Required = header.IsRequired,
                                Schema = new OpenApiSchema { Type = "string" },
                                Description = header.IsRequired ? "Required header" : "Optional header"
                            });
                        }
                    }
                }
            }
        }
    }
}
