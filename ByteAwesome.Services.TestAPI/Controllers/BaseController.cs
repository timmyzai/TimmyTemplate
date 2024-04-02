using ByteAwesome.Services.TestAPI.Helper;
using ByteAwesome.Services.TestAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.Services.TestAPI.Controllers
{
    public interface IBaseController<TEntityDto, TCreateDto, TKey>
    {
        Task<ActionResult<ResponseDto<TEntityDto>>> Add([FromBody] TCreateDto input);
        Task<ActionResult<ResponseDto<TEntityDto>>> Delete(TKey id);
        Task<ActionResult<ResponseDto<TEntityDto>>> GetById(TKey id);
        Task<ActionResult<ResponseDto<TEntityDto>>> Update([FromBody] TEntityDto input);
    }
    [ApiVersion("1.0")]
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class BaseController<TEntityDto, TCreateDto, TKey, TRepository> : Controller, IBaseController<TEntityDto, TCreateDto, TKey> where TRepository : IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        protected readonly TRepository repository;
        public BaseController(TRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        [ApiVersion("1.0")]
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> GetById(TKey id)
        {
            string logErrorMessage = string.Format(LanguageService.Translate(ErrorCodes.General.EntityGetById),id);
            var response = new ResponseDto<TEntityDto>();
            try
            {
                var result = await repository.GetById(id);
                response.Result = result;
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

        [HttpPost]
        [ApiVersion("1.0")]
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> Add([FromBody] TCreateDto input)
        {
            string logErrorMessage = LanguageService.Translate(ErrorCodes.General.EntityAdd);
            var response = new ResponseDto<TEntityDto>();
            try
            {
                response.Result = await repository.Add(input);
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

        [HttpPut]
        [ApiVersion("1.0")]
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> Update([FromBody] TEntityDto input)
        {
            string logErrorMessage = LanguageService.Translate(ErrorCodes.General.EntityUpdate);
            var response = new ResponseDto<TEntityDto>();
            try
            {
                var result = await repository.Update(input);
                response.Result = result;
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

        [HttpDelete]
        [ApiVersion("1.0")]
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> Delete(TKey id)
        {
            string logErrorMessage = LanguageService.Translate(ErrorCodes.General.EntityDelete);
            var response = new ResponseDto<TEntityDto>();
            try
            {
                var result = await repository.Delete(id);
                response.Result = result;
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
