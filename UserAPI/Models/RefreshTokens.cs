
using AwesomeProject;

namespace UserAPI.Models
{
    public class RefreshToken : Entity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}