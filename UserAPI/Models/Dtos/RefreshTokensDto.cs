
using AwesomeProject;

namespace UserAPI.Models.Dtos
{
    public class RefreshTokenDto : EntityDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}