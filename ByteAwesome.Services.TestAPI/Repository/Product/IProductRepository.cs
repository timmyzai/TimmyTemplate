using ByteAwesome.Services.TestAPI.Helper;
using ByteAwesome.Services.TestAPI.Models.Dtos;

namespace ByteAwesome.Services.TestAPI.Repository
{
    public interface IProductRepository : IBaseRepository<ProductDto, CreateProductDto, Guid>
    {
    }
}
