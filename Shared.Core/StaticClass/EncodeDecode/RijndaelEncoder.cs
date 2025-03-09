using System.Security.Cryptography;
using System.Text;

namespace AwesomeProject
{
    public static class RijndaelEncoder
    {
        private static readonly int KeySize = 256;
        private static readonly int BlockSize = 128;
        public static string EncryptString(string toEncryptText, string key)
        {
            if (toEncryptText is null) return null;
            byte[] encrypted;
            using (RijndaelManaged rijAlg = new())
            {
                rijAlg.KeySize = KeySize;
                rijAlg.BlockSize = BlockSize;
                rijAlg.Key = Encoding.UTF8.GetBytes(key);
                rijAlg.GenerateIV();

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(rijAlg.IV, 0, rijAlg.IV.Length); // Prepend IV

                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(toEncryptText);
                        }
                    }

                    encrypted = msEncrypt.ToArray();
                }
            }
            return Convert.ToBase64String(encrypted);
        }
        public static string DecryptString(string encryptedText, string key)
        {
            if (encryptedText is null) return null;
            string plaintext = null;
            byte[] buffer = Convert.FromBase64String(encryptedText);

            using (RijndaelManaged rijAlg = new())
            {
                rijAlg.KeySize = KeySize;
                rijAlg.BlockSize = BlockSize;
                rijAlg.Key = Encoding.UTF8.GetBytes(key);

                byte[] iv = new byte[rijAlg.BlockSize / 8];
                Array.Copy(buffer, 0, iv, 0, iv.Length);
                rijAlg.IV = iv;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(buffer, iv.Length, buffer.Length - iv.Length))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
