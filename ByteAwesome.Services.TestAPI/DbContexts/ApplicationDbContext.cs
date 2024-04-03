using Microsoft.EntityFrameworkCore;
using ByteAwesome.Services.TestAPI.Models;


namespace ByteAwesome.Services.TestAPI.DbContexts
{
    public partial class ApplicationDbContext : DbContext
    {
        private readonly DbContextOptions<ApplicationDbContext> options;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor
        ) : base(options)
        {
            this.options = options;
            this.httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Product> Product { get; set; }
    }
}
