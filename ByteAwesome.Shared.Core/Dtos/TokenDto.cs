using Microsoft.IdentityModel.Tokens;

namespace ByteAwesome
{
    public class TokenConfigurationDto
    {
        public bool IsEnabled { get; set; }
        public SymmetricSecurityKey SecurityKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public SigningCredentials SigningCredentials { get; set; }

        public TimeSpan Expiration { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
    }
}