using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome
{
    public interface ICRUD_AdminBaseController<TEntityDto, TCreateDto, TKey>
    {
        Task<ActionResult<ResponseDto<TEntityDto>>> Add([FromBody] TCreateDto input);
        Task<ActionResult<ResponseDto<TEntityDto>>> Delete(TKey id);
        Task<ActionResult<ResponseDto<TEntityDto>>> GetById(TKey id);
        Task<ActionResult<ResponseDto<TEntityDto>>> Update([FromBody] TEntityDto input);
    }
    
    public class CRUD_AdminBaseController<TEntityDto, TCreateDto, TKey, TRepository> : AdminBaseController, ICRUD_AdminBaseController<TEntityDto, TCreateDto, TKey> where TRepository : IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        protected readonly TRepository repository;
        public CRUD_AdminBaseController(TRepository repository) { this.repository = repository; }

        [HttpGet]
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> GetById(TKey id)
        {
            string logErrorMessage = string.Format(LanguageService.Translate(ErrorCodes.General.EntityGetById), id);
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
