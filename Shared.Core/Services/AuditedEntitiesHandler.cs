using System.Linq.Expressions;
using AwesomeProject;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class AuditingService
    {
        private readonly DbContext context;
        public bool IsAuditingEnabled { get; set; } = true;

        public AuditingService(DbContext context)
        {
            this.context = context;
        }
        public void ProcessAuditedEntities()
        {
            if (!IsAuditingEnabled) return;

            var auditedEntries = context.ChangeTracker.Entries<IAuditedEntity>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            if (!auditedEntries.Any()) return;

            var currentTime = DateTime.UtcNow;
            string userName = CurrentSession.GetUserName() ?? "system";
            foreach (var entry in auditedEntries)
            {
                var entity = entry.Entity;
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedTime = currentTime;
                        entity.CreatedBy = userName;
                        break;
                    case EntityState.Modified:
                        entry.Property(nameof(IAuditedEntity.CreatedTime)).IsModified = false;
                        entry.Property(nameof(IAuditedEntity.CreatedBy)).IsModified = false;
                        entity.LastModifiedTime = currentTime;
                        entity.LastModifiedBy = userName;
                        break;
                    case EntityState.Deleted:
                        if (entity is IFullyAuditedEntity fullyAuditedEntity)
                        {
                            fullyAuditedEntity.IsDeleted = true;
                            fullyAuditedEntity.DeletedTime = currentTime;
                            fullyAuditedEntity.DeletedBy = userName;
                            entry.State = EntityState.Modified;
                        }
                        break;
                }
            }
        }

        public void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IFullyAuditedEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(GetIsNotDeletedFilter(entityType.ClrType));
                }
            }
        }

        private static LambdaExpression GetIsNotDeletedFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(IFullyAuditedEntity.IsDeleted));
            var condition = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            return Expression.Lambda(condition, parameter);
        }
    }
}
