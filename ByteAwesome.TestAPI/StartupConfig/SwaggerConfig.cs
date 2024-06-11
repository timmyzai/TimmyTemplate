using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Any;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.TestAPI
{
    public partial class Startup
    {
        private void ConfigureSwagger(IServiceCollection services)
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
                options.OperationFilter<RemoveVersionFromParameter>();
                options.OperationFilter<AddAcceptLanguageHeaderParameter>();
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                options.DocInclusionPredicate((version, desc) =>
                {
                    if (!desc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var types = new List<Type> { methodInfo.DeclaringType };
                    var baseType = methodInfo.DeclaringType.BaseType;

                    while (baseType != null)
                    {
                        types.Add(baseType);
                        baseType = baseType.BaseType;
                    }

                    var versions = types
                        .SelectMany(t => t.GetCustomAttributes(true))
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v.ToString()}" == version);
                });
            });

        }
        protected class SwaggerEnumSchemaFilter : ISchemaFilter
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
        protected class AddAcceptLanguageHeaderParameter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (operation.Parameters == null)
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
        protected class RemoveVersionFromParameter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");
                if (versionParameter != null)
                    operation.Parameters.Remove(versionParameter);
            }
        }
        protected class ReplaceVersionWithExactValueInPath : IDocumentFilter
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
        protected OpenApiSecurityRequirement CreateSecurityRequirement() => new OpenApiSecurityRequirement
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
    }
}
