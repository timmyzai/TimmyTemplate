using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using AwesomeProject;

namespace SecretAPI.Models.Dtos
{
    public class CreateOtpDto
    {
        [Required]
        public string Value { get; set; }
        [Required]
        public OtpType Type { get; set; }
        [Required]
        public OtpActionType ActionType { get; set; }
    }
    public class OtpDto : AuditedEntityDto<Guid>
    {
        public string Value { get; set; }
        public string TacCode { get; set; }
        public OtpType Type { get; set; }
        public OtpActionType ActionType { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public enum OtpType
    {
        [Description("SMS")]
        SMS = 0,
        [Description("Email")]
        Email = 1,
    }

    public enum OtpActionType
    {
        [Description("Verify email for first time login")]
        EmailVerification = 0,
        [Description("Verify phone number")]
        PhoneVerification = 1,
        [Description("User forgot password")]
        ForgotPassword = 2,
        [Description("User login verification")]
        Login = 3
    }

    public class VerifyOTPDto
    {
        [Required]
        public string Value { get; set; }
        [Required]
        public string TacCode { get; set; }
        [Required]
        public OtpActionType ActionType { get; set; }
    }
}
