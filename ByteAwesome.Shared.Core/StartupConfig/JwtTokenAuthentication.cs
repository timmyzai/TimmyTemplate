
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ByteAwesome.StartupConfig
{
    public partial class StartUpHelper
    {
        public static TokenConfiguration ConfigureAuthentication(IConfiguration configuration, IServiceCollection services)
        {
            var tokenConfig = new TokenConfiguration
            {
                IsEnabled = bool.Parse(configuration["Authorization:IsEnabled"]),
                SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Authorization:SecurityKey"])),
                Issuer = configuration["Authorization:Issuer"],
                Audience = configuration["Authorization:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Authorization:SecurityKey"])), SecurityAlgorithms.HmacSha256)
            };
            if (double.TryParse(configuration["Authorization:TokenExpiry"], out double tokenExpiryMinutes))
            {
                tokenConfig.Expiration = TimeSpan.FromMinutes(tokenExpiryMinutes);
            }
            if (tokenConfig.IsEnabled)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    SetTokenValidationParameters(options, tokenConfig);
                    ConfigureTokenDecryption(options, configuration);
                });
            }
            return tokenConfig;
        }
        protected static void SetTokenValidationParameters(JwtBearerOptions options, TokenConfiguration tokenConfig)
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
        }
        protected static void ConfigureTokenDecryption(JwtBearerOptions options, IConfiguration configuration)
        {
            string encryptionKey = configuration["App:EncryptSecretKey"];
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var encryptedToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", string.Empty);
                    if (!string.IsNullOrEmpty(encryptedToken) && encryptedToken != "null")
                    {
                        try
                        {
                            context.Token = AesEncoder.DecryptString(encryptedToken, encryptionKey);
                            context.HttpContext.Items["DecryptedToken"] = context.Token;
                        }
                        catch
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        }
    }
}
