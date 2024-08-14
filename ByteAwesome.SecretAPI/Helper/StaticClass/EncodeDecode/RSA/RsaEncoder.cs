using System.Security.Cryptography;
using System.Text;

namespace ByteAwesome.SecretAPI.Helper
{
    public class RsaEncoder
    {
        public static RSA_Keys GenerateRsaKeys()
        {
            int maxAttempts = 10;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                using (RSA rsa = RSA.Create(2048))
                {
                    string publicKey = ExportPublicKeyToPem(rsa);
                    string privateKey = rsa.ToXmlString(true);

                    if (!KeyStore.Exists(publicKey))
                    {
                        KeyStore.Add(publicKey, privateKey);
                        return new RSA_Keys
                        {
                            PublicKey = publicKey, //use to encrypt data
                            PrivateKey = privateKey //use to decrypt data
                        };
                    }
                }
            }
            throw new InvalidOperationException("Failed to generate a unique cryptographic key after " + maxAttempts + " attempts.");
        }
        public static string Encrypt_Chunk(string message, string pem)
        {
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(message);
            int maxChunkSize = 190;

            List<byte[]> chunks = new List<byte[]>();
            for (int i = 0; i < dataToEncrypt.Length; i += maxChunkSize)
            {
                int chunkSize = Math.Min(maxChunkSize, dataToEncrypt.Length - i);
                byte[] chunk = new byte[chunkSize];
                Buffer.BlockCopy(dataToEncrypt, i, chunk, 0, chunkSize);
                chunks.Add(chunk);
            }
            List<byte[]> encryptedChunks = new List<byte[]>();
            using (RSA publicKeyRsa = ImportPublicKeyFromPem(pem))
            {
                foreach (var chunk in chunks)
                {
                    encryptedChunks.Add(publicKeyRsa.Encrypt(chunk, RSAEncryptionPadding.OaepSHA256));
                }
            }
            byte[] encryptedData = new byte[encryptedChunks.Sum(c => c.Length)];
            int offset = 0;
            foreach (var chunk in encryptedChunks)
            {
                Buffer.BlockCopy(chunk, 0, encryptedData, offset, chunk.Length);
                offset += chunk.Length;
            }
            string encryptedMessage = Convert.ToBase64String(encryptedData);
            return encryptedMessage;
        }
        public static string Decrypt_Chunk(string encryptedData, string privateKey)
        {
            byte[] encryptedDataByte = Convert.FromBase64String(encryptedData);
            List<byte[]> decryptedChunks = new List<byte[]>();

            using (RSA rsaDecrypt = RSA.Create())
            {
                rsaDecrypt.FromXmlString(privateKey);
                int chunkSize = rsaDecrypt.KeySize / 8;
                for (int i = 0; i < encryptedDataByte.Length; i += chunkSize)
                {
                    byte[] chunk = new byte[chunkSize];
                    Buffer.BlockCopy(encryptedDataByte, i, chunk, 0, chunkSize);
                    decryptedChunks.Add(rsaDecrypt.Decrypt(chunk, RSAEncryptionPadding.OaepSHA256));
                }
            }
            byte[] decryptedData = new byte[decryptedChunks.Sum(c => c.Length)];
            int offset = 0;
            foreach (var chunk in decryptedChunks)
            {
                Buffer.BlockCopy(chunk, 0, decryptedData, offset, chunk.Length);
                offset += chunk.Length;
            }
            string decryptedMessage = Encoding.UTF8.GetString(decryptedData);
            return decryptedMessage;
        }
        public static string ConvertToJsonBase64(string json)
        {
            // Convert JSON string to bytes
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // Convert bytes to Base64 string
            string base64Json = Convert.ToBase64String(jsonBytes);

            return base64Json;
        }
        public static string Encrypt(string message, string pem)
        {
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(message);
            using (RSA publicKeyRsa = ImportPublicKeyFromPem(pem))
            {
                var encryptedDataByte = publicKeyRsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.Pkcs1);
                var encryptedData = Convert.ToBase64String(encryptedDataByte);
                return encryptedData;
            }
        }
        public static string Decrypt(string encryptedData, string privateKey)
        {
            byte[] encryptedDataByte = Convert.FromBase64String(encryptedData);
            using (RSA rsaDecrypt = RSA.Create())
            {
                rsaDecrypt.FromXmlString(privateKey);
                var decryptedDataByte = rsaDecrypt.Decrypt(encryptedDataByte, RSAEncryptionPadding.Pkcs1);
                var decryptedData = Encoding.UTF8.GetString(decryptedDataByte);
                return decryptedData;
            }
        }

        static string ExportPublicKeyToPem(RSA rsa)
        {
            var publicKey = rsa.ExportSubjectPublicKeyInfo();
            return ConvertToPem("PUBLIC KEY", publicKey);
        }
        private static string ConvertToPem(string keyType, byte[] keyBytes)
        {
            var base64Key = Convert.ToBase64String(keyBytes);
            var sb = new StringBuilder();
            sb.AppendLine($"-----BEGIN {keyType}-----");
            for (int i = 0; i < base64Key.Length; i += 64)
            {
                sb.AppendLine(base64Key.Substring(i, Math.Min(64, base64Key.Length - i)));
            }
            sb.AppendLine($"-----END {keyType}-----");
            return sb.ToString();
        }
        static RSA ImportPublicKeyFromPem(string pem)
        {
            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(DecodePem(pem, "PUBLIC KEY"), out _);
            return rsa;
        }

        private static byte[] DecodePem(string pem, string keyType)
        {
            var pemHeader = $"-----BEGIN {keyType}-----";
            var pemFooter = $"-----END {keyType}-----";
            var start = pem.IndexOf(pemHeader, StringComparison.Ordinal) + pemHeader.Length;
            var end = pem.IndexOf(pemFooter, start, StringComparison.Ordinal);
            var base64 = pem[start..end].Trim();
            return Convert.FromBase64String(base64);
        }
    }
}
