using ByteAwesome.TestAPI.entity;
using ByteAwesome.TestAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ByteAwesome.TestAPI.DbContexts;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // context.Database.EnsureCreated();
        context.Database.Migrate();

        if (!context.Brands.Any())
        {   
            Brand[] brands = {
                new()
                {
                    Id = 0,
                    Name = "CASH MARKET"
                }
            };
            context.Brands.AddRange(brands);
            context.SaveChanges();
        }

        if (!context.Suppliers.Any())
        {
            Supplier[] suppliers = {
                new()
                {
                    Id = 0,
                    Name = "CASH Supplier"
                }
            };
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();
        }
        if (!context.ProductTypes.Any())
        {
            ProductType[] productTypes = {
                new()
                {
                    Id = 0,
                    Name = "FOOD"
                }
            };
            context.ProductTypes.AddRange(productTypes);
            context.SaveChanges();
        }
    }
    
}