using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ByteAwesome.TestAPI.DbContexts
{
    public partial class ApplicationDbContext : DbContext
    {
        public override int SaveChanges()
        {
            ProcessAuditedEntities();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ProcessAuditedEntities();
            return base.SaveChangesAsync();
        }
        private void ProcessAuditedEntities()
        {
            var AuditedEntries = ChangeTracker.Entries<IAuditedEntity>();
            if (AuditedEntries.Any())
            {
                ProcessAuditedEntities(AuditedEntries);
            }
        }

        private void ProcessAuditedEntities(IEnumerable<EntityEntry> entries)
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
}
