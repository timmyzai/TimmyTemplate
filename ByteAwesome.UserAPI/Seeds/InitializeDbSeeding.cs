using ByteAwesome.UserAPI.Seed.RoleAndUser;
using ByteAwesome.UserAPI.DbContexts;


namespace ByteAwesome.UserAPI.Seed
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
            new RoleAndUserCreator(context).Create();
        }
    }
}
