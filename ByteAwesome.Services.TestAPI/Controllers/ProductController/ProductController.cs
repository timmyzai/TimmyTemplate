using ByteAwesome.Services.TestAPI.Helper;
using ByteAwesome.Services.TestAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Microsoft.Extensions.Options;
using ByteAwesome.Services.TestAPI.Modules;
using Microsoft.AspNetCore.Authorization;
using ByteAwesome.Services.TestAPI.Repository;


namespace ByteAwesome.Services.TestAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/[controller]/[action]")]
    public class ProductController : BaseController<ProductDto, CreateProductDto, Guid, IProductRepository>, IProductController
    {
        private readonly IProductRepository _repository;

        public ProductController(
            IProductRepository repository
        ): base(repository)
        {

            _repository = repository;
        }

        [HttpGet]
        [ApiVersion("1.0")]
        public async Task<ActionResult<ResponseDto<IEnumerable<ProductDto>>>> GetRange()
        {
            var response = new ResponseDto<IEnumerable<ProductDto>>();
            string logErrorMessage = $"{ErrorCodes.Secret.GenerateAccessToken} - {LanguageService.Translate(ErrorCodes.Secret.GenerateAccessToken)}";
            try
            {
                response.Result = await _repository.Get();
            }
            catch (AppException ex)
            {
                ActionResultHandler.HandleException(ex, response, logErrorMessage, ex.StatusCode, ex.JsonData);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex, response, logErrorMessage);
            }
            return Json(response);
        }
        
        
    }
}
