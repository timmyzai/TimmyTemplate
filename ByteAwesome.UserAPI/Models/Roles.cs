

namespace ByteAwesome.UserAPI.Models
{
    public class Roles : FullyAuditedEntity<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}