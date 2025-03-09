using Services;
using UserAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace UserAPI.DbContexts
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
        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _auditingService.ApplyGlobalFilters(modelBuilder);
        }
    }
}
