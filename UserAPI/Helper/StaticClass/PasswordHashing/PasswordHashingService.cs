using System.Security.Cryptography;

namespace UserAPI.Helper
{
    public static class PwdHashTokenHelper
    {
        private const int PBKDF2IterCount = 100000;
        private const int PBKDF2SubkeyLength = 20;
        private const int SaltSize = 16;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public static (string Hash, string Salt) CreateHash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, PBKDF2IterCount, HashAlgorithm))
            {
                var subkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
                return (Convert.ToBase64String(subkey), Convert.ToBase64String(salt));
            }
        }

        public static byte[] CreateHash(string salt, string password)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using (var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, PBKDF2IterCount, HashAlgorithm))
            {
                return deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }
        }

        public static bool VerifyHashedPassword(string hashedPassword, string salt, string password)
        {
            var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
            var computedHash = CreateHash(salt, password);
            return hashedPasswordBytes.SequenceEqual(computedHash);
        }
    }
}
