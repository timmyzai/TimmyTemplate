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

        public DbSet<Games> Games { get; set; }
        public DbSet<GameScores> GameScores { get; set; }
        public DbSet<ScoreRules> ScoreRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _auditingService.ApplyGlobalFilters(modelBuilder);
        }
    }
}
