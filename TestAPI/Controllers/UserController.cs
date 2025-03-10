using TestAPI.Dtos.User;
using TestAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

using AwesomeProject;

namespace TestAPI.Controllers;

public class UserController : BaseController<UserDto, CreateUserDto, Guid, IUserRepository>
{
    private readonly IUserRepository _repository;
    
    public UserController(IUserRepository repository) : base(repository)
    {
        _repository = repository;
    }
    
    public async Task<ActionResult<ResponseDto<IEnumerable<UserDto>>>> GetAll()
    {
        var response = new ResponseDto<IEnumerable<UserDto>>();
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
}