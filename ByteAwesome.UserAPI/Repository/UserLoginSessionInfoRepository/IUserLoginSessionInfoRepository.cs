using ByteAwesome.UserAPI.Models.Dtos;

namespace ByteAwesome.UserAPI.Repository
{
    public interface IUserLoginSessionInfoRepository : IBaseRepository<UserLoginSessionInfoDto, UserLoginSessionInfoDto, Guid>
    {
        Task<IEnumerable<UserLoginSessionInfoDto>> GetByUserId(Guid userId);
        Task CleanUpOldSessions();
    }
}