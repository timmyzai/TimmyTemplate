using SecretAPI.Models;
using Services;
using Microsoft.EntityFrameworkCore;

namespace SecretAPI.DbContexts
{
    public partial class ApplicationDbContext : DbContext
    {
        private readonly AuditingService _auditingService;
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
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
        public DbSet<TwoFactorAuth> TwoFactorAuth { get; set; }
        public DbSet<Otp> Otp { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _auditingService.ApplyGlobalFilters(modelBuilder);
        }
    }
}
