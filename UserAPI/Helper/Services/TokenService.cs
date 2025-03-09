using AwesomeProject;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services
{
    public interface ITokenService
    {
        Guid GetUserIdFromExpiredToken(HttpRequest request);
    }
    public class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;

        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Guid GetUserIdFromExpiredToken(HttpRequest request)
        {
            string encryptionKey = configuration["App:EncryptSecretKey"];
            var decryptedToken = GeneralHelper.DecryptAccessToken(request, encryptionKey);
            var Key = Encoding.UTF8.GetBytes(configuration["Authorization:SecurityKey"]);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ClockSkew = TimeSpan.Zero
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(decryptedToken, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
    }
}