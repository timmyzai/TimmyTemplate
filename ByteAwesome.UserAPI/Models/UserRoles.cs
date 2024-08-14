using System.ComponentModel.DataAnnotations.Schema;



namespace ByteAwesome.UserAPI.Models
{
    public class UserRoles : FullyAuditedEntity<Guid>
    {
        [ForeignKey("UsersId")]
        public Guid UsersId { get; set; }
        public virtual Users Users { get; set; }
        [ForeignKey("RolesId")]
        public Guid RolesId { get; set; }
        public virtual Roles Roles { get; set; }
    }
}