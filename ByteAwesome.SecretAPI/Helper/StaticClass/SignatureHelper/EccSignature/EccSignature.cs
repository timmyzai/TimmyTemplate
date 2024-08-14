using System.Security.Cryptography;
using System.Text;

namespace ByteAwesome.SecretAPI.Helper
{
    #region Ecc Generate Keys with/without User Id, Sinature Sign and Verify 
    public class EccSignature
    {
        private static ECCurve curve = ECCurve.NamedCurves.nistP256;
        public static ECC_Keys GenerateEccKeys()
        {
            int maxAttempts = 10;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                using (var ecdsa = ECDsa.Create(curve))
                {
                    var publicKeyParams = ecdsa.ExportParameters(false);
                    var privateKeyParams = ecdsa.ExportParameters(true);

                    string publicKey = Convert.ToBase64String(publicKeyParams.Q.X) + Convert.ToBase64String(publicKeyParams.Q.Y);
                    string privateKey = Convert.ToBase64String(privateKeyParams.D);
                    if (!KeyStore.Exists(publicKey))
                    {
                        KeyStore.Add(publicKey, privateKey);
                        return new ECC_Keys
                        {
                            PublicKey = publicKey, //use to verify data
                            PrivateKey = privateKey //use to sign data
                        };
                    }
                }
            }
            throw new InvalidOperationException("Failed to generate a unique cryptographic key after " + maxAttempts + " attempts.");
        }
        public static ECC_Keys GenerateEccKeysWithUserId(Guid userId)
        {
            int maxAttempts = 10;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                using (var ecc = ECDsa.Create(curve))
                {
                    byte[] userIdBytes = userId.ToByteArray();
                    byte[] salt = CreateSalt();
                    var derivedKey = DeriveKey(userIdBytes, salt, ecc.KeySize / 8);
                    using (var eccDerived = ECDsa.Create(curve))
                    {
                        var parameters = new ECParameters
                        {
                            Curve = curve,
                            D = derivedKey
                        };
                        parameters.Validate();
                        eccDerived.ImportParameters(parameters);
                        var publicKeyParams = eccDerived.ExportParameters(false);
                        var privateKeyParams = eccDerived.ExportParameters(true);

                        string publicKey = Convert.ToBase64String(publicKeyParams.Q.X) + Convert.ToBase64String(publicKeyParams.Q.Y);
                        string privateKey = Convert.ToBase64String(privateKeyParams.D);
                        if (!KeyStore.Exists(publicKey))
                        {
                            KeyStore.Add(publicKey, privateKey);
                            return new ECC_Keys
                            {
                                PublicKey = publicKey, //use to verify data
                                PrivateKey = privateKey //use to sign data
                            };
                        }
                    }
                }
            }
            throw new InvalidOperationException("Failed to generate a unique cryptographic key after " + maxAttempts + " attempts.");
        }
        private static byte[] DeriveKey(byte[] userId, byte[] salt, int keySize)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(userId, salt, 10000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(keySize);
            }
        }
        private static byte[] CreateSalt()
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        public static string SignData(string data, string privateKey)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            using (var ecc = ECDsa.Create(curve))
            {
                ecc.ImportParameters(new ECParameters
                {
                    Curve = curve,
                    D = Convert.FromBase64String(privateKey)
                });
                byte[] signature = ecc.SignData(dataBytes, HashAlgorithmName.SHA256);
                return Convert.ToBase64String(signature);
            }
        }
        public static bool VerifySignature(string data, string signatureBase64, string publicKey)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (var ecc = ECDsa.Create(curve))
            {
                var keyParts = ExtractPublicKeyParts(publicKey);
                ecc.ImportParameters(new ECParameters
                {
                    Curve = curve,
                    Q = {
                X = Convert.FromBase64String(keyParts.Item1),
                Y = Convert.FromBase64String(keyParts.Item2)
            }
                });
                byte[] signature = Convert.FromBase64String(signatureBase64);
                return ecc.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256);
            }
        }
        private static Tuple<string, string> ExtractPublicKeyParts(string publicKey)
        {
            int midPoint = publicKey.Length / 2;
            string xPart = publicKey.Substring(0, midPoint);
            string yPart = publicKey.Substring(midPoint);
            return Tuple.Create(xPart, yPart);
        }
    }
    #endregion
}