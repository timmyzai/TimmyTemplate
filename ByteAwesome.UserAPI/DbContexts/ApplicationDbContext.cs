using ByteAwesome.Services;
using ByteAwesome.UserAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ByteAwesome.UserAPI.DbContexts
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
        public DbSet<Kyc> Kyc { get; set; }
        public DbSet<KycDocs> KycDocs { get; set; }
        public DbSet<UserLoginSessionInfo> UserLoginSessionInfo { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _auditingService.ApplyGlobalFilters(modelBuilder);

            modelBuilder.Entity<UserLoginSessionInfo>()
                .HasQueryFilter(uls => !uls.Users.IsDeleted);
        }
    }
}
