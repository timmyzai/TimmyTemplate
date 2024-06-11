using Microsoft.IdentityModel.Tokens;

namespace ByteAwesome
{
    public class TokenConfiguration
    {
        public Boolean IsEnabled { get; set; }
        public SymmetricSecurityKey SecurityKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public SigningCredentials SigningCredentials { get; set; }

        public TimeSpan Expiration { get; set; }
    }
}