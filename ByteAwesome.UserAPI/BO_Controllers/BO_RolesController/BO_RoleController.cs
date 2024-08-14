using Microsoft.AspNetCore.Mvc;
using ByteAwesome.UserAPI.Repository;
using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI.Controllers
{
    public class BO_RoleController : CRUD_AdminBaseController<RolesDto, CreateRolesDto, Guid, IRoleRepository>
    {
        public BO_RoleController(IRoleRepository repository) : base(repository) { }

        public override async Task<ActionResult<ResponseDto<RolesDto>>> Add([FromBody] CreateRolesDto input)
        {
            var response = new ResponseDto<RolesDto>();
            try
            {
                var existingRole = await repository.GetRoleByName(input.Name);
                if (existingRole is not null)
                {
                    throw new AppException(ErrorCodes.User.RoleAlreadyExists);
                }
                var result = await repository.Add(input);
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
        public async Task<ActionResult<ResponseDto<IEnumerable<RolesDto>>>> Get(PaginationRequestDto input)
        {
            var response = new ResponseDto<IEnumerable<RolesDto>>();
            try
            {
                var items = await repository.Get(input);
                var pagedResult = PagedList<RolesDto>.ToPagedList(items, input);
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