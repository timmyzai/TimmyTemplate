namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class CreateRolesDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class RolesDto : FullyAuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}