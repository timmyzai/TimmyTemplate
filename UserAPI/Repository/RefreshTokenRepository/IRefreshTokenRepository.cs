using UserAPI.Models.Dtos;
using AwesomeProject;

namespace UserAPI.Repository
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshTokenDto, RefreshTokenDto, int>
    {
        Task<RefreshTokenDto> GetByUserId(Guid userId);
        Task RemoveByUserId(Guid userId);
    }
}