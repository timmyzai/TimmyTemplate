using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace ByteAwesome
{
    public class RijndaelEncoder
    {
        private static readonly int KeySize = 256;
        private static readonly int BlockSize = 128;
        public static string EncryptString(string toEncryptText, string secretPhrase)
        {
            try
            {
                if (toEncryptText == null) return null;
                byte[] encrypted;
                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.KeySize = KeySize;
                    rijAlg.BlockSize = BlockSize;
                    rijAlg.Key = Encoding.UTF8.GetBytes(secretPhrase);
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
            catch (Exception ex)
            {
                Log.Error(ex, ErrorCodes.General.EncryptString);
                throw new AppException(ErrorCodes.General.DecryptString, ex.Message);
            }
        }

        public static string DecryptString(string encryptedText, string secretPhrase)
        {
            try
            {
                if (encryptedText == null) return null;
                string plaintext = null;
                byte[] buffer = Convert.FromBase64String(encryptedText);
                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.KeySize = KeySize;
                    rijAlg.BlockSize = BlockSize;
                    rijAlg.Key = Encoding.UTF8.GetBytes(secretPhrase);

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
            catch (Exception ex)
            {
                Log.Error(ex, ErrorCodes.General.DecryptString);
                throw new AppException(ErrorCodes.General.DecryptString, ex.Message);
            }
        }
    }
}
