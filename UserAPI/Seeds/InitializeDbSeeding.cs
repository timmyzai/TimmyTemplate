using UserAPI.Seed.RoleAndUser;
using UserAPI.DbContexts;

namespace UserAPI.Seed
{
    public class SeedHelper
    {
        public static void SeedHostDb(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                SeedData(context);
            }
        }

        private static void SeedData(ApplicationDbContext context)
        {
            new RoleAndUserSeedCreator(context).Create();
        }
    }
}
