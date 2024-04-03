using AutoMapper;
using ByteAwesome.Services.TestAPI.DbContexts;
using ByteAwesome.Services.TestAPI.Models;
using ByteAwesome.Services.TestAPI.Models.Dtos;
using ByteAwesome.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ByteAwesome.Services.TestAPI.Helper;

namespace ByteAwesome.Services.TestAPI.Repository
{
    public class ProductRepository : BaseRepository<Product, ProductDto, CreateProductDto, Guid>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
       
    }
}
