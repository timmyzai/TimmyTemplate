using SecretAPI.Models.Dtos;

using AwesomeProject;

namespace SecretAPI.Models
{
    public class Otp : AuditedEntity<Guid>
    {
        public string Value { get; set; }
        public string TacCode { get; set; }
        public bool IsActive { get; set; } = true;
        public OtpType Type { get; set; }
        public OtpActionType ActionType { get; set; }
        public Otp()
        {
            TacCode = new Random().Next(100000, 999999).ToString();
        }
    }
}