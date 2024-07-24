using ByteAwesome.Services;
using Microsoft.EntityFrameworkCore;

namespace ByteAwesome.TestAPI.DbContexts
{
    public partial class ApplicationDbContext : DbContext
    {
        private readonly DbContextOptions<ApplicationDbContext> options;
        private readonly AuditingService _auditingService;
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
            this.options = options;
            _auditingService = new AuditingService(this);
        }
        #region Overrides
        public override int SaveChanges()
        {
            _auditingService.ProcessAuditedEntities();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _auditingService.ProcessAuditedEntities();
            return base.SaveChangesAsync();
        }
        #endregion
    }
}
