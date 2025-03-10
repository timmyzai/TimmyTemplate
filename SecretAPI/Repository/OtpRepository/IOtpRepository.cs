using SecretAPI.Models.Dtos;

using AwesomeProject;

namespace SecretAPI.Repository
{
    public interface IOtpRepository : IBaseRepository<OtpDto, CreateOtpDto, Guid>
    {
        Task<OtpDto> GetValidOtpByValueAndType(string value, OtpActionType ActionType);
    }
}
