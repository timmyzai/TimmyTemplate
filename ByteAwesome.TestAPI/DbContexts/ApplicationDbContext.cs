using Microsoft.EntityFrameworkCore;

namespace ByteAwesome.TestAPI.DbContexts
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
    }
}
