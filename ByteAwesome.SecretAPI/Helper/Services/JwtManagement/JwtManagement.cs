using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ByteAwesome.SecretAPI.Grpc;
using ByteAwesome.SecretAPI.Models.Dtos;
using ByteAwesome.SecretAPI.Modules;
using ByteAwesome.Services;
using Microsoft.Extensions.Options;

namespace ByteAwesome.SecretAPI.Helper
{
    public interface IJwtManagement
    {
        List<Claim> CreateClaims(GenerateAccessTokenProtoDto input);
        Task<GenerateAccessTokenResult> CreateTokenAsync(GenerateAccessTokenProtoDto input, TimeSpan? expiration = null);
    }

    public class JwtManagement : IJwtManagement
    {
        private readonly TokenConfiguration tokenConfiguration;
        private readonly IRedisCacheService redisCacheService;
        private readonly string _encryptSecretKey;

        public JwtManagement(
            TokenConfiguration tokenConfiguration,
            IOptions<AppModuleConfig> appConfig,
            IRedisCacheService redisCacheService
        )
        {
            this.tokenConfiguration = tokenConfiguration;
            this.redisCacheService = redisCacheService;
            _encryptSecretKey = appConfig.Value.EncryptSecretKey;
        }

        public async Task<GenerateAccessTokenResult> CreateTokenAsync(GenerateAccessTokenProtoDto input, TimeSpan? expiration = null)
        {
            var claims = CreateClaims(input);
            var now = DateTime.UtcNow;
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: tokenConfiguration.Issuer,
                audience: tokenConfiguration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? tokenConfiguration.Expiration),
                signingCredentials: tokenConfiguration.SigningCredentials
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            await SaveOrUpdateTokenInRedis(input.UserLoginSerssionId, accessToken);

            var result = new GenerateAccessTokenResult
            {
                EncryptedAccessToken = AesEncoder.EncryptString(accessToken, _encryptSecretKey),
                ExpireInSeconds = (int)tokenConfiguration.Expiration.TotalSeconds
            };
            return result;
        }
        public List<Claim> CreateClaims(GenerateAccessTokenProtoDto input)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, input.UserName),
                    new Claim(ClaimTypes.NameIdentifier, input.UserId),
                    new Claim(ClaimTypes.Email, input.EmailAddress),
                    new Claim(ClaimTypes.MobilePhone, input.PhoneNumber),
                    new Claim("UserLoginSessionId", input.UserLoginSerssionId)
                };

            claims.AddRange(input.RoleNames.Select(role => new Claim(ClaimTypes.Role, role)));

            claims.AddRange(new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, input.UserId),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                });
            return claims;
        }
        private async Task SaveOrUpdateTokenInRedis(string userLoginSessionId, string accessToken)
        {
            await redisCacheService.SetAsync(userLoginSessionId, accessToken, tokenConfiguration.Expiration);
        }
    }
}