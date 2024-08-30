using AutoMapper;
using ByteAwesome.TestAPI.DbContexts;
using ByteAwesome.TestAPI.entity;
using ByteAwesome.TestAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ByteAwesome.TestAPI.Repository
{
    
    public class ProductRepository :BaseRepository<Product,ProductDto,ProductCreate,int>
    {
        public ProductRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}

