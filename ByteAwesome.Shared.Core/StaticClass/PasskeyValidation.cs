using System.Text.Json;
using Fido2NetLib;

namespace ByteAwesome
{
    public class PasskeyValidatorHelper
    {
        public static void ValidateAssertion(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                throw new AppException(ErrorCodes.User.PasskeyAssertionError);
            }
            try
            {
                var assertion = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(jsonStr);
            }
            catch
            {
                throw new AppException(ErrorCodes.User.PasskeyAssertionError);
            }
        }
        public static void ValidateAttetation(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                throw new AppException(ErrorCodes.User.PasskeyAttestationError);
            }
            try
            {
                var attestation = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(jsonStr);
            }
            catch
            {
                throw new AppException(ErrorCodes.User.PasskeyAttestationError);
            }
        }
    }
}