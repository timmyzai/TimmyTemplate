using System.ComponentModel.DataAnnotations;
using Fido2NetLib;

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class CreatePassKeyDto : RequiredTwoFactorPin
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public AuthenticatorAttestationRawResponse PendingCreateCredential { get; set; }
    }
    public class EnableDisablePassKeyDto
    {
        [Required]
        public bool IsEnable { get; set; }
        public AuthenticatorAssertionRawResponse PendingVerifyCredential { get; set; }
    }
    public class LoginWithPassKeyDto : LoginIdentityDto
    {
        public AuthenticatorAssertionRawResponse PendingVerifyCredential { get; set; }
    }
}