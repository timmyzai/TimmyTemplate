using ByteAwesome.TestAPI.Models;
using ByteAwesome.TestAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.TestAPI.Controllers.ProductController;

[AllowAnonymous]
public class ProductController : CRUD_BaseController<ProductDto, ProductCreate, Guid,ProductRepository>
{
     private readonly ProductRepository _repository;
     public virtual async Task<ActionResult<ResponseDto<IEnumerable<ProductDto>>>> GetAllProduct()
     {
         var response = new ResponseDto<IEnumerable<ProductDto>>();
         try
         {
             var result = await _repository.Get();
             response.Result = result;
         }
         catch (AppException ex)
         {
             ActionResultHandler.HandleException(ex, response, ex.Message, ex.StatusCode, ex.JsonData);
         }
         catch (Exception ex)
         {
             ActionResultHandler.HandleException(ex, response);
         }
         return Json(response);
     }

    public ProductController(ProductRepository repository) : base(repository)
    {
        _repository = repository;
    }
}