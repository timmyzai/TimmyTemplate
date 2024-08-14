using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class VerifyPhoneNumberDto
    {
        [Required]
        public string TacCode { get; set; }
    }
}
