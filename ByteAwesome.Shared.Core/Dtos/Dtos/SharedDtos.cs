using System.ComponentModel.DataAnnotations;

namespace ByteAwesome
{
    public class RequiredTwoFactorPin
    {
        [Required]
        public string TwoFactorPin { get; set; }
    }
    public class OptionalTwoFactorPin
    {
        public string TwoFactorPin { get; set; }
    }
}