using Serilog.Enrichers.Sensitive;
using System.Text.RegularExpressions;

namespace AwesomeProject
{
    public class EmailMaskingOperator : IMaskingOperator
    {
        private readonly Regex _emailRegex = new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.IgnoreCase);

        public MaskingResult Mask(string input, string mask)
        {
            var match = _emailRegex.Match(input);
            if (!match.Success) return MaskingResult.NoMatch;

            var maskedEmail = MaskEmail(input);
            return new MaskingResult { Match = true, Result = maskedEmail };
        }

        private static string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            return string.Concat(new string('*', atIndex), email.AsSpan(atIndex));
        }
    }

    public class PhoneNumberMaskingOperator : IMaskingOperator
    {
        private readonly Regex _phoneRegex = new(@"^\+(?:[0-9] ?){6,14}[0-9]$", RegexOptions.IgnoreCase);

        public MaskingResult Mask(string input, string mask)
        {
            var match = _phoneRegex.Match(input);
            if (!match.Success) return MaskingResult.NoMatch;

            var maskedPhone = MaskPhoneNumber(input);
            return new MaskingResult { Match = true, Result = maskedPhone };
        }

        private static string MaskPhoneNumber(string phoneNumber)
        {
            return string.Concat(new string('*', phoneNumber.Length - 4), phoneNumber.AsSpan(phoneNumber.Length - 4));
        }
    }
}