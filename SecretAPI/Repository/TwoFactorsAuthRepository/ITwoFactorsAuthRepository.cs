using SecretAPI.Models.Dtos;

using AwesomeProject;

namespace SecretAPI.Repository
{
    public interface ITwoFactorAuthRepository : IBaseRepository<TwoFactorAuthDto, CreateTwoFactorAuthDto, Guid>
    {
        Task<TwoFactorAuthDto> GetByUserId(Guid userId);
    }
}