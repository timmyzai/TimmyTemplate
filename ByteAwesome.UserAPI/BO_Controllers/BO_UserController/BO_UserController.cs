using Microsoft.AspNetCore.Mvc;
using ByteAwesome.UserAPI.Repository;
using ByteAwesome.UserAPI.Models.Dtos;
using Serilog;
using ByteAwesome.UserAPI.Services;

namespace ByteAwesome.UserAPI.Controllers
{
    public class BO_UserController : CRUD_AdminBaseController<UserDto, CreateUserDto, Guid, IUserRepository>
    {
        private readonly IUserRolesRepository userRolesRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IUserServices userServices;
        public BO_UserController(
            IUserRepository repository,
            IUserRolesRepository userRolesRepository,
            IRoleRepository roleRepository,
            IUserServices userServices

        ) : base(repository)
        {
            this.userRolesRepository = userRolesRepository;
            this.roleRepository = roleRepository;
            this.userServices = userServices;
        }
        public async override Task<ActionResult<ResponseDto<UserDto>>> Add([FromBody] CreateUserDto input)
        {
            Log.Information("User Registration process started for the User : " + input.EmailAddress);
            var response = new ResponseDto<UserDto>();
            try
            {
                #region Add User
                await repository.CheckIfUserExists(input.UserName, input.PhoneNumber, input.EmailAddress);
                var result = await repository.Add(input);
                #endregion

                #region Add User Roles Junction Table
                if (input.RoleIdList is not null && input.RoleIdList.Count > 0)
                {
                    foreach (var roleId in input.RoleIdList)
                    {
                        var userRolesDto = new CreateUserRolesDto()
                        {
                            UsersId = result.Id,
                            RolesId = roleId
                        };
                        await userRolesRepository.Add(userRolesDto);
                    }
                }
                else
                {
                    var role = await roleRepository.GetRoleByName(RoleNames.Clients);
                    var userRolesDto = new CreateUserRolesDto()
                    {
                        UsersId = result.Id,
                        RolesId = role.Id
                    };
                    await userRolesRepository.Add(userRolesDto);
                }
                #endregion

                await userServices.SendConfirmationEmail(result, HttpContext);
                response.DisplayMessage = "Registered successfully. Confirmation email will be sent.";
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
        public async override Task<ActionResult<ResponseDto<UserDto>>> Update([FromBody] UserDto input)
        {
            var existingUser = await repository.GetById(input.Id);
            if (existingUser is null)
            {
                throw new AppException(ErrorCodes.User.UserNotExists, $"User {input.Id} not Exists.");
            }
            if (existingUser.UserData.UserName != input.UserData.UserName)
            {
                existingUser = await repository.GetUserByUserLoginIdentity(input.UserData.UserName);
                if (existingUser is not null)
                {
                    throw new AppException(ErrorCodes.User.IdentityTypeAlreadyExists, $"User {input.UserData.UserName} already Exists.");
                }
            }
            if (existingUser.UserData.EmailAddress != input.UserData.EmailAddress)
            {
                existingUser = await repository.GetUserByUserLoginIdentity(input.UserData.EmailAddress);
                if (existingUser is not null)
                {
                    throw new AppException(ErrorCodes.User.IdentityTypeAlreadyExists, $"User {input.UserData.EmailAddress} already Exists.");
                }
            }
            if (existingUser.UserData.PhoneNumber != input.UserData.PhoneNumber)
            {
                existingUser = await repository.GetUserByUserLoginIdentity(input.UserData.PhoneNumber);
                if (existingUser is not null)
                {
                    throw new AppException(ErrorCodes.User.IdentityTypeAlreadyExists, $"User {input.UserData.PhoneNumber} already Exists.");
                }
            }
            var response = await base.Update(input);
            #region Add User Roles Junction Table
            if (input.UserData.RoleNames is not null && input.UserData.RoleNames.Count > 0)
            {
                foreach (var roleName in input.UserData.RoleNames)
                {
                    var role = await roleRepository.GetRoleByName(roleName);
                    var userRole = await userRolesRepository.GetUserRoleByUser(input.Id);
                    userRole.RolesId = role.Id;
                    await userRolesRepository.Update(userRole);
                }
            }
            #endregion
            return Json(response);
        }
        public async Task<ActionResult<ResponseDto<IEnumerable<UserDto>>>> Get(PaginationRequestDto input)
        {
            var response = new ResponseDto<IEnumerable<UserDto>>();
            try
            {
                var items = await repository.Get(input);
                var pagedResult = PagedList<UserDto>.ToPagedList(items, input);
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
