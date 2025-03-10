using Services;
using Microsoft.EntityFrameworkCore;
using TestAPI.Models;

namespace TestAPI.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        private readonly AuditingService _auditingService;
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
            _auditingService = new AuditingService(this);
        }

        public DbSet<Wallet> Wallet { get; set; }
        public DbSet<ExchangeRate> ExchangeRate { get; set; }
        public DbSet<BaseCurrency> BaseCurrency { get; set; }
        public DbSet<User> User { get; set; }

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _auditingService.ApplyGlobalFilters(modelBuilder);
        }
    }
}
