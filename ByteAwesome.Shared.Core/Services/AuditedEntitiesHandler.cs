using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ByteAwesome.Services
{
    public class AuditingService
    {
        private readonly DbContext context;

        public AuditingService(DbContext context)
        {
            this.context = context;
        }
        public void ProcessAuditedEntities()
        {
            var AuditedEntries = context.ChangeTracker.Entries<IAuditedEntity>();
            if (AuditedEntries.Any())
            {
                ProcessAuditedEntities(AuditedEntries);
            }
            void ProcessAuditedEntities(IEnumerable<EntityEntry> entries)
            {
                var addedEntities = entries.Where(entry => entry.State == EntityState.Added).Select(entry => entry.Entity).ToList();
                var modifiedEntities = entries.Where(entry => entry.State == EntityState.Modified).Select(entry => entry.Entity).ToList();
                var currentTime = DateTime.UtcNow;
                string userName = CurrentSession.GetUserName() ?? "system";
                if (addedEntities.Any())
                {
                    foreach (var entity in addedEntities)
                    {
                        if (entity is IAuditedEntity IntAuditedEntity)
                        {
                            IntAuditedEntity.CreatedTime = currentTime;
                            IntAuditedEntity.CreatedBy = userName;
                        }
                    }
                }
                if (modifiedEntities.Any())
                {
                    foreach (var entity in modifiedEntities)
                    {
                        if (entity is IAuditedEntity AuditedEntity)
                        {
                            AuditedEntity.LastModifiedTime = currentTime;
                            AuditedEntity.LastModifiedBy = userName;
                        }
                        if (entity is IFullyAuditedEntity FullyAuditedEntity)
                        {
                            if (FullyAuditedEntity.IsDeleted == true)
                            {
                                FullyAuditedEntity.DeletedTime = currentTime;
                                FullyAuditedEntity.DeletedBy = userName;
                            }
                        }
                    }
                }
            }
        }
        public void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IFullyAuditedEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var filter = GetFilterExpression(entityType.ClrType);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
            LambdaExpression GetFilterExpression(Type entityType) //soft delete filter
            {
                var parameter = Expression.Parameter(entityType, "e");
                var property = Expression.Property(parameter, nameof(IFullyAuditedEntity.IsDeleted));
                var condition = Expression.MakeBinary(ExpressionType.Equal, property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);
                return lambda;
            }
        }
    }
}
