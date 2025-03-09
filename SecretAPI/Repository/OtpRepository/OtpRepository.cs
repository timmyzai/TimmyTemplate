using AutoMapper;
using SecretAPI.DbContexts;
using SecretAPI.Models;
using SecretAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;

using AwesomeProject;

namespace SecretAPI.Repository
{
    public class OtpRepository : BaseRepository<Otp, OtpDto, CreateOtpDto, Guid>, IOtpRepository
    {
        public OtpRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
        public async Task<OtpDto> GetValidOtpByValueAndType(string value, OtpActionType ActionType)
        {
            var item = await Query()
                                    .OrderByDescending(r => r.CreatedTime)
                                    .FirstOrDefaultAsync(r => r.Value == value && r.ActionType == ActionType);
            if (item is null)
            {
                return default;
            }
            return MapEntityToDto(item);
        }
    }
}
