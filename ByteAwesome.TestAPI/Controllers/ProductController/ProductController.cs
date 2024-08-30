using ByteAwesome.TestAPI.Models;
using ByteAwesome.TestAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.TestAPI.Controllers.ProductController;

[AllowAnonymous]
public class ProductController : BaseController
{
    private readonly ICRUD_BaseController<ProductDto, ProductCreate, int> _productController;
    private readonly ProductRepository _repository;
    
    public ProductController(ICRUD_BaseController<ProductDto, ProductCreate, int> productController,ProductRepository repository)
    {
        _productController = productController;
        _repository = repository;
    }

    
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
    
    [HttpGet]
    public async Task<ActionResult<ResponseDto<ProductDto>>> GetProduct([FromQuery] int productId)
    {
        var response = new ResponseDto<ResponseDto<ProductDto>>();

        try
        {
                var res = await _productController.GetById(productId);
                if (res.Result is JsonResult jsonResult)
                {
                    var product = jsonResult.Value as ResponseDto<ProductDto>;
                    response.Result = product;
                }

                return Json(response);
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

    [HttpPost]
    public async Task<ActionResult<ResponseDto<ProductDto>>> AddProduct([FromBody] ProductCreate product)
    {
        var response = new ResponseDto<ResponseDto<ProductDto>>();

        try
        {
            
                var res = await _productController.Add(product);
                if (res.Result is JsonResult jsonResult)
                {
                    var productResult = jsonResult.Value as ResponseDto<ProductDto>;
                    response.Result = productResult;
                }

                return Json(response);
                
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
    
    [HttpPost]
    public async Task<ActionResult<ResponseDto<ProductDto>>> UpdateProduct([FromBody] ProductDto product)
    {
        var response = new ResponseDto<ResponseDto<ProductDto>>();

        try
        {
            
            var res = await _productController.Update(product);
            if (res.Result is JsonResult jsonResult)
            {
                var productResult = jsonResult.Value as ResponseDto<ProductDto>;
                response.Result = productResult;
            }

            return Json(response);
                
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
    
    
    [HttpPost]
    public async Task<ActionResult<ResponseDto<ProductDto>>> DeleteProduct([FromQuery] int productId)
    {
        var response = new ResponseDto<ResponseDto<ProductDto>>();

        try
        {
            
            var res = await _productController.Delete(productId);
            if (res.Result is JsonResult jsonResult)
            {
                var productResult = jsonResult.Value as ResponseDto<ProductDto>;
                response.Result = productResult;
            }

            return Json(response);
                
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
}