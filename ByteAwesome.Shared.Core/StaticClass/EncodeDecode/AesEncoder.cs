using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace ByteAwesome
{
    public class AesEncoder
    {
        public static string DecryptString(string encryptedText, string key)
        {
            try
            {
                if (encryptedText == null) return null;
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(encryptedText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ErrorCodes.General.DecryptString);
                throw new AppException(ErrorCodes.General.DecryptString, ex.Message);
            }
        }
    }
}
