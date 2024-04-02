using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using AutoMapper;
using ByteAwesome.Services.TestAPI.Helper;
using ByteAwesome.Services.TestAPI.Modules;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.Services.TestAPI
{
    public partial class Startup
    {
        private void PreInitialize(IServiceCollection services)
        {
            IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
            services.AddSingleton(mapper);

            var tokenConfig = ConfigureAuthentication(services);
            services.AddSingleton(tokenConfig);
            services.AddSingleton(MappingConfig.RegisterMaps().CreateMapper());

            services.Configure<AppModuleConfig>(configuration.GetSection("App"));
        }

        private TokenConfiguration ConfigureAuthentication(IServiceCollection services)
        {
            var tokenConfig = new TokenConfiguration
            {
                IsEnabled = bool.Parse(configuration["Authorization:IsEnabled"]),
                SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Authorization:SecurityKey"])),
                Issuer = configuration["Authorization:Issuer"],
                Audience = configuration["Authorization:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Authorization:SecurityKey"])), SecurityAlgorithms.HmacSha256),
                Expiration = TimeSpan.FromDays(365)
            };

            if (tokenConfig.IsEnabled)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.Audience = tokenConfig.Audience;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = tokenConfig.Issuer,
                        ValidAudience = tokenConfig.Audience,
                        IssuerSigningKey = tokenConfig.SecurityKey,
                        ClockSkew = TimeSpan.Zero
                    };

                    string key = configuration["App:EncryptSecretKey"];
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var encryptedToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", string.Empty);
                            if (!string.IsNullOrEmpty(encryptedToken) && encryptedToken != "null")
                            {
                                try
                                {
                                    context.Token = EncodeDecode.DecryptString(encryptedToken, key);
                                }
                                catch
                                {
                                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                }
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            }

            return tokenConfig;
        }
        public class ValidateModelAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                if (!context.ModelState.IsValid)
                {
                    var errorMessages = context.ModelState
                        .SelectMany(ms => ms.Value.Errors)
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "A validation error occurred." : e.ErrorMessage)
                        .ToArray();

                    var combinedErrorMessage = string.Join("; ", errorMessages);

                    var errorResponse = new ResponseDto<object>
                    {
                        IsSuccess = false,
                        DisplayMessage = "Validation errors occurred.",
                        Error = new ErrorDto
                        {
                            StatusCode = ErrorCodes.General.InvalidField,
                            ErrorMessage = combinedErrorMessage
                        },
                        Result = null
                    };

                    context.Result = new BadRequestObjectResult(errorResponse);
                }
            }
        }
    }
}