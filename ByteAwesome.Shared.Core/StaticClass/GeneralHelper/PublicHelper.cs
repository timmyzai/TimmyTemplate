using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace ByteAwesome
{
    public class GeneralHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            return attributes != null && attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
        public static string GenerateCustomGuid(string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            string result = input + Guid.NewGuid().ToString();
            return result;
        }
        public static string GenerateRandomSixStrNums()
        {
            string result = (new Random().Next(100000, 999999)).ToString();
            return result;
        }
        public static decimal TrimZeroAfterDecimal(decimal number)
        {
            if (number % 1 == 0) //Check if num has decimals
            {
                return number;
            }
            else
            {
                return decimal.Parse(number.ToString().TrimEnd('0'));
            }
        }
        public static int CountNumOfDecimalPlaces(decimal number)
        {
            return BitConverter.GetBytes(decimal.GetBits(number)[3])[2];
        }
        public static decimal TruncateNumToXDecimals(decimal qty, int x)
        {
            var number = (decimal)Math.Pow(10, x);
            return Math.Truncate(qty * number) / number;
        }
        public static string SetEnum<TEnum>(string value, out TEnum? enumValue) where TEnum : struct
        {
            if (Enum.TryParse(value, true, out TEnum result))
            {
                enumValue = result;
            }
            else
            {
                enumValue = null;
            }
            return value;
        }
        public static string CreateHashedIdentifier(string identifier)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(identifier));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public static bool IsJwtTokenFormat(string token)
        {
            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                return jwtToken != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}