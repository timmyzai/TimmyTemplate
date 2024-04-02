namespace ByteAwesome.Services.EntitiesDto
{
    public abstract class EntityDto<TKey> : IEntityDto<TKey>
    {
        public TKey Id { get; set; }
    }
    public abstract class AuditedEntityDto<TKey> : EntityDto<TKey>, IAuditedEntityDto
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedTime { get; set; }
    }
    public abstract class FullyAuditedEntityDto<TKey> : AuditedEntityDto<TKey>, IFullyAuditedEntityDto
    {
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
    public interface IEntityDto<TKey>
    {
        TKey Id { get; set; }
    }
    public interface IAuditedEntityDto
    {
        string CreatedBy { get; set; }
        DateTime CreatedTime { get; set; }
        string LastModifiedBy { get; set; }
        DateTime? LastModifiedTime { get; set; }
    }
    public interface IFullyAuditedEntityDto : IAuditedEntityDto
    {
        bool IsDeleted { get; set; }
        string DeletedBy { get; set; }
        DateTime? DeletedTime { get; set; }
    }
    public abstract class EntityDto : EntityDto<int> { }
    public abstract class AuditedEntityDto : AuditedEntityDto<int> { }
    public abstract class FullyAuditedEntityDto : FullyAuditedEntityDto<int> { }
}
