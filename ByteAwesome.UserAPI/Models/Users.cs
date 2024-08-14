

namespace ByteAwesome.UserAPI.Models
{
    public class Users : FullyAuditedEntity<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string PassWord { get; set; }
        public string PasswordSalt { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public bool IsPassKeyEnabled { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsLockedOut { get; set; }
        public int PasswordTryCount { get; set; }
        public ICollection<UserRoles> Roles { get; internal set; }
        public Kyc KycData { get; set; }
    }
}