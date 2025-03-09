using System.Security.Claims;
using System.Security.Cryptography;
using SecretAPI.Models.Dtosƒ;
using SecretAPI.Modules;
using Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using AwesomeProject;
using SecretAPI.Grpc;

namespace SecretAPI.Helper
{
    public interface IJwtManagement
    {
        List<Claim> CreateClaims(AuthenticationClaimsProtoDto input);
        Task<GenerateAccessTokenProtoResult> CreateTokenAsync(AuthenticationClaimsProtoDto input, TimeSpan? expiration = null);
        RefreshTokenDto GenerateRefreshToken();
    }

    public class JwtManagement : IJwtManagement
    {
        private readonly TokenConfigurationDto tokenConfiguration;
        private readonly IRedisCacheService redisCacheService;
        private readonly string _encryptSecretKey;

        public JwtManagement(
            TokenConfigurationDto tokenConfiguration,
            IOptions<AppModuleConfig> appConfig,
            IRedisCacheService redisCacheService
        )
        {
            this.tokenConfiguration = tokenConfiguration;
            this.redisCacheService = redisCacheService;
            _encryptSecretKey = appConfig.Value.EncryptSecretKey;
        }

        public async Task<GenerateAccessTokenProtoResult> CreateTokenAsync(AuthenticationClaimsProtoDto input, TimeSpan? expiration = null)
        {
            var claims = CreateClaims(input);
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = tokenConfiguration.Issuer,
                Audience = tokenConfiguration.Audience,
                Subject = new ClaimsIdentity(claims),
                NotBefore = now,
                Expires = now.Add(expiration ?? tokenConfiguration.Expiration),
                SigningCredentials = tokenConfiguration.SigningCredentials
            };

            var accessToken = new JsonWebTokenHandler().CreateToken(tokenDescriptor);

            await SaveOrUpdateTokenInRedis(accessToken);

            var result = new GenerateAccessTokenProtoResult
            {
                EncryptedAccessToken = RijndaelEncoder.EncryptString(accessToken, _encryptSecretKey),
                ExpireInSeconds = (int)tokenConfiguration.Expiration.TotalSeconds
            };
            return result;
        }

        public List<Claim> CreateClaims(AuthenticationClaimsProtoDto input)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, input.UserName),
                new(ClaimTypes.NameIdentifier, input.UserId),
                new(ClaimTypes.Email, input.EmailAddress),
                new(ClaimTypes.MobilePhone, input.PhoneNumber)
            };

            claims.AddRange(input.RoleNames.Select(role => new Claim(ClaimTypes.Role, role)));

            claims.AddRange(
            [
                new Claim(JwtRegisteredClaimNames.Sub, input.UserId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            ]);

            return claims;
        }
        public RefreshTokenDto GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var result = new RefreshTokenDto
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpireDate = DateTime.UtcNow.AddDays(tokenConfiguration.RefreshTokenExpirationDays)
            };
            return result;
        }
        private async Task SaveOrUpdateTokenInRedis(string accessToken)
        {
            string userLoginSessionId = Guid.NewGuid().ToString();
            await redisCacheService.SetAsync(userLoginSessionId, accessToken, tokenConfiguration.Expiration);
        }
    }
}
