using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome
{
    public static class GeneralHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            try
            {
                var fieldInfo = value.GetType().GetField(value.ToString());
                if (fieldInfo is null) throw new ArgumentException("Invalid enum value", nameof(value));

                var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attributes.Length > 0 ? attributes[0].Description : value.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve enum description.", ex);
            }
        }
        public static string GenerateCustomGuid(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            return input + Guid.NewGuid().ToString();
        }
        public static decimal TrimZeroAfterDecimal(decimal number)
        {
            try
            {
                return number % 1 == 0 ? number : decimal.Parse(number.ToString().TrimEnd('0'));
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Error trimming zeroes after decimal.", ex);
            }
        }
        public static int CountNumOfDecimalPlaces(decimal number)
        {
            return BitConverter.GetBytes(decimal.GetBits(number)[3])[2];
        }
        public static decimal TruncateNumToXDecimals(decimal qty, int x)
        {
            if (x < 0) throw new ArgumentOutOfRangeException(nameof(x), "Decimal places must be non-negative.");

            var factor = (decimal)Math.Pow(10, x);
            return Math.Truncate(qty * factor) / factor;
        }
        public static string SetEnum<TEnum>(string value, out TEnum? enumValue) where TEnum : struct
        {
            if (!Enum.TryParse(value, true, out TEnum result))
            {
                enumValue = null;
            }
            enumValue = result;
            return value;
        }
        public static bool IsJwtTokenFormat(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                return jwtToken is not null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid JWT format.", ex);
            }
        }
        public static List<string> DiscoverApiVersions<Startup>(string microServiceApiName, string defaultVersion = "v1.0")
        {
            var apiVersions = typeof(Startup).Assembly.GetTypes()
                .Where(type => type.Namespace is not null && type.Namespace.Equals($"ByteAwesome.{microServiceApiName}.Controllers"))
                .Where(type => type.GetCustomAttributes(typeof(ApiVersionAttribute), true).Any())
                .SelectMany(type => type.GetCustomAttributes(typeof(ApiVersionAttribute), true))
                .Cast<ApiVersionAttribute>()
                .SelectMany(attr => attr.Versions)
                .Distinct()
                .Select(v => $"v{v}")
                .ToList();
            if (!apiVersions.Contains(defaultVersion)) apiVersions.Insert(0, defaultVersion);
            return apiVersions;
        }
        public static SortedDictionary<string, object> ConvertDtoToSortedDictionary<TDto>(TDto requestDto)
        {
            if (requestDto is null)
                throw new ArgumentNullException(nameof(requestDto), "The request DTO cannot be null.");

            var dictionary = new SortedDictionary<string, object>();
            PropertyInfo[] properties = typeof(TDto).GetProperties();
            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(requestDto, null);
                    if (value is not null)
                    {
                        string camelCaseName = ToCamelCase(property.Name);
                        dictionary[camelCaseName] = value;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing property {property.Name} of DTO type {typeof(TDto).Name}.", ex);
                }
            }
            return dictionary;
        }

        public static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentException("Input string cannot be null or empty.", nameof(str));

            return str.Length > 1 ? char.ToLowerInvariant(str[0]) + str.Substring(1) : str.ToLowerInvariant();
        }
        public static bool IsDevelopmentEnvironment()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        }
        public static bool IsProductionEnvironment()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        }
        public static bool IsStagingEnvironment()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Staging";
        }
    }
}