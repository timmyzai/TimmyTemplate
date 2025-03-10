using UserAPI.Models.Dtos;
using AwesomeProject;

namespace UserAPI.Services
{
    public class UserValidationClass
    {
        public static void ValidateUser(UserDto user)
        {
            if (user is null)
            {
                throw new AppException(ErrorCodes.User.InvalidUserLoginIdentity);
            }
            if (!user.UserData.IsActive)
            {
                throw new AppException(ErrorCodes.User.UserIsNotActive);
            }
            if (user.UserData.IsLockedOut)
            {
                throw new AppException(ErrorCodes.User.UserLockedOut);
            }
        }
    }
}