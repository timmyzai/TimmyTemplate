using ByteAwesome.Services.TestAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.Services.TestAPI.Controllers
{
    public interface IProductController
    {
        Task<ActionResult<ResponseDto<IEnumerable<ProductDto>>>> GetRange();
    }
}
