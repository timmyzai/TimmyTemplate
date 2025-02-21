using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ByteAwesome.StartupConfig
{
    public partial class StartUpHelper
    {
        public static TokenConfigurationDto ConfigureAuthentication(IConfiguration configuration, IServiceCollection services)
        {
            var tokenConfig = new TokenConfigurationDto
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
            if (int.TryParse(configuration["Authorization:RefreshTokenExpiryDays"], out int refreshTokenExpiryDays))
            {
                tokenConfig.RefreshTokenExpirationDays = refreshTokenExpiryDays;
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
        protected static void SetTokenValidationParameters(JwtBearerOptions options, TokenConfigurationDto tokenConfig)
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
                OnMessageReceived = async context =>
                {
                    try
                    {
                        context.Token = GeneralHelper.DecryptAccessToken(context.Request, encryptionKey);
                        context.HttpContext.Items["DecryptedToken"] = context.Token;
                    }
                    catch (Exception ex)
                    {
                        await context.HandleAccessTokenDecryptionFailure(ex); 
                    }
                }
            };
        }
    }
}
