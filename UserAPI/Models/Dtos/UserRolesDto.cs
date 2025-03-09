
using AwesomeProject;

namespace UserAPI.Models.Dtos
{
    public class CreateUserRolesDto
    {
        public Guid UsersId { get; set; }
        public Guid RolesId { get; set; }
    }
    public class UserRolesDto : FullyAuditedEntityDto<Guid>
    {
        public Guid UsersId { get; set; }
        public Guid RolesId { get; set; }
    }
}