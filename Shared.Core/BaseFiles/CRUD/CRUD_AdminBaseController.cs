using Microsoft.AspNetCore.Mvc;

namespace AwesomeProject
{
    public interface ICRUD_BaseAdminController<TEntityDto, TCreateDto, TKey>
    {
        Task<ActionResult<ResponseDto<TEntityDto>>> Add([FromBody] TCreateDto input);
        Task<ActionResult<ResponseDto<TEntityDto>>> Delete(TKey id);
        Task<ActionResult<ResponseDto<TEntityDto>>> GetById(TKey id);
        Task<ActionResult<ResponseDto<TEntityDto>>> Update([FromBody] TEntityDto input);
    }

    public class CRUD_BaseAdminController<TEntityDto, TCreateDto, TKey, TRepository> : BaseAdminController, ICRUD_BaseAdminController<TEntityDto, TCreateDto, TKey> where TRepository : IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        protected readonly TRepository repository;
        public CRUD_BaseAdminController(TRepository repository) { this.repository = repository; }
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> GetById(TKey id)
        {
            var response = new ResponseDto<TEntityDto>();
            try
            {
                var result = await repository.GetById(id);
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
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> Add([FromBody] TCreateDto input)
        {
            var response = new ResponseDto<TEntityDto>();
            try
            {
                response.Result = await repository.Add(input);
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
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> Update([FromBody] TEntityDto input)
        {
            var response = new ResponseDto<TEntityDto>();
            try
            {
                var result = await repository.Update(input);
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
        public virtual async Task<ActionResult<ResponseDto<TEntityDto>>> Delete(TKey id)
        {
            var response = new ResponseDto<TEntityDto>();
            try
            {
                var result = await repository.Delete(id);
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
    }
}
