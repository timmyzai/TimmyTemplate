using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByteAwesome
{
    public abstract class Entity<TKey> : IEntity<TKey>
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual TKey Id { get; set; }
    }
    public abstract class AuditedEntity<TKey> : Entity<TKey>, IAuditedEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedTime { get; set; }
    }
    public abstract class FullyAuditedEntity<TKey> : AuditedEntity<TKey>, IFullyAuditedEntity
    {
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
    public interface IAuditedEntity
    {
        string CreatedBy { get; set; }
        DateTime CreatedTime { get; set; }
        string LastModifiedBy { get; set; }
        DateTime? LastModifiedTime { get; set; }
    }
    public interface IFullyAuditedEntity : IAuditedEntity
    {
        bool IsDeleted { get; set; }
        string DeletedBy { get; set; }
        DateTime? DeletedTime { get; set; }
    }
    public interface IUserIdEntity
    {
        Guid UserId { get; set; }
    }
    public abstract class Entity : Entity<int> { }
    public abstract class AuditedEntity : AuditedEntity<int> { }
    public abstract class FullyAuditedEntity : FullyAuditedEntity<int> { }
}
