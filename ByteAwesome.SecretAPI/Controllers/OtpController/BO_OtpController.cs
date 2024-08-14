using ByteAwesome.SecretAPI.Models.Dtos;
using ByteAwesome.SecretAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome.SecretAPI.Controllers
{
    public class OtpController : CRUD_AdminBaseController<OtpDto, CreateOtpDto, Guid, IOtpRepository>
    {
        public OtpController(IOtpRepository repository) : base(repository) { }

        public async Task<ActionResult<ResponseDto<IEnumerable<OtpDto>>>> Get(PaginationRequestDto input)
        {
            var response = new ResponseDto<IEnumerable<OtpDto>>();
            try
            {
                var items = await repository.Get(input);
                var pagedResult = PagedList<OtpDto>.ToPagedList(items, input);
                response.Result = pagedResult.Items;
                response.SetPaginationMetadata(pagedResult);
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
