using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Any;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace ByteAwesome.StartupConfig
{
    public partial class StartUpHelper
    {
        public static void ConfigureSwagger(List<string> ApiVersions, string _microServiceApiName, IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                foreach (var version in ApiVersions)
                {
                    options.SwaggerDoc(version, new OpenApiInfo { Title = $"ByteAwesome.{_microServiceApiName}", Version = version });
                }

                options.EnableAnnotations();
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter 'Bearer' [space] and your token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(CreateSecurityRequirement());
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPath>();
                options.DocInclusionPredicate((version, desc) => HasValidApiVersion(version, desc));
                options.OperationFilter<RemoveVersionFromParameter>();
                options.OperationFilter<AddAcceptLanguageHeaderParameter>();
                options.OperationFilter<InjectFromHeaderOperationFilter>();
            });
        }
        private static bool HasValidApiVersion(string version, ApiDescription desc)
        {
            if (!desc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
            var versions = new List<ApiVersion>();
            for (var type = methodInfo.DeclaringType; type is not null; type = type.BaseType)
            {
                versions.AddRange(type.GetCustomAttributes<ApiVersionAttribute>(true)
                                       .SelectMany(attr => attr.Versions));
            }
            return versions.Any(v => $"v{v}" == version) || (versions.Count == 0 && version == "v1.0");
        }
        private static OpenApiSecurityRequirement CreateSecurityRequirement() => new OpenApiSecurityRequirement
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
        };
        private class SwaggerEnumSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                var type = context.Type;
                if (!type.IsEnum || schema.Extensions.ContainsKey("x-enumNames"))
                {
                    return;
                }

                var enumNames = new OpenApiArray();
                enumNames.AddRange(Enum.GetNames(type).Select(name => new OpenApiString(name)));
                schema.Extensions.Add("x-enumNames", enumNames);
            }
        }
        private class AddAcceptLanguageHeaderParameter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (operation.Parameters is null)
                    operation.Parameters = new List<OpenApiParameter>();

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
        private class RemoveVersionFromParameter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");
                if (versionParameter is not null)
                    operation.Parameters.Remove(versionParameter);
            }
        }
        private class ReplaceVersionWithExactValueInPath : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                var paths = new OpenApiPaths();
                foreach (var path in swaggerDoc.Paths)
                {
                    paths.Add(path.Key.Replace("v{version}", swaggerDoc.Info.Version), path.Value);
                }
                swaggerDoc.Paths = paths;
            }
        }
        private class InjectFromHeaderOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var methodInfo = context.MethodInfo;
                var injectHeaderAttributes = methodInfo.GetCustomAttributes<InjectFromHeaderAttribute>(true);

                foreach (var attr in injectHeaderAttributes)
                {
                    foreach (var header in attr.headers)
                    {
                        var existingParam = operation.Parameters.FirstOrDefault(p => p.Name == header.Name);
                        if (existingParam is not null)
                        {
                            // Update existing parameter if already added (e.g., by another filter or previous operations)
                            existingParam.Required = existingParam.Required || header.IsRequired;
                        }
                        else
                        {
                            // Add new parameter
                            operation.Parameters.Add(new OpenApiParameter
                            {
                                Name = header.Name,
                                In = ParameterLocation.Header,
                                Required = header.IsRequired,
                                Schema = new OpenApiSchema
                                {
                                    Type = "string"
                                },
                                Description = header.IsRequired ? "Required header" : "Optional header"
                            });
                        }
                    }
                }
            }
        }
    }
}
