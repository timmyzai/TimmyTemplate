using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Any;

namespace ByteAwesome.Services.TestAPI
{
    public partial class Startup
    {
        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(_defaultApiVersion, new OpenApiInfo { Title = $"ByteAwesome.Services.{_microServiceApiName}", Version = _defaultApiVersion });
                options.EnableAnnotations();
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter 'Bearer' [space] and your token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                
                options.OperationFilter<AddAcceptLanguageHeaderParameter>();
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                                {
                                    new OpenApiSecurityScheme
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type=ReferenceType.SecurityScheme,
                                            Id="Bearer"
                                        },
                                        Scheme="oauth2",
                                        Name="Bearer",
                                        In=ParameterLocation.Header
                                    },
                                    new List<string>()
                                }
                });
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
            });
        }

        public class SwaggerEnumSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                var type = context.Type;
                if (!type.IsEnum || schema.Extensions.ContainsKey("x-enumNames"))
                {
                    return;
                }

                var enumNames = new OpenApiArray();
                enumNames.AddRange(Enum.GetNames(type).Select(_ => new OpenApiString(_)));
                schema.Extensions.Add("x-enumNames", enumNames);
            }
        }
        public class AddAcceptLanguageHeaderParameter : IOperationFilter
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
                    Schema = new OpenApiSchema
                    {
                        Type = "String"
                    },
                    Description = "Language code"
                });
            }
        }
    }
}
