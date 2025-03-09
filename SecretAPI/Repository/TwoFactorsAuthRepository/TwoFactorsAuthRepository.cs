using AutoMapper;
using SecretAPI.DbContexts;
using SecretAPI.Models;
using SecretAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;

using AwesomeProject;

namespace SecretAPI.Repository
{
    public class TwoFactorAuthRepository : BaseRepository<TwoFactorAuth, TwoFactorAuthDto, CreateTwoFactorAuthDto, Guid>, ITwoFactorAuthRepository
    {
        public TwoFactorAuthRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        { }

        public async Task<TwoFactorAuthDto> GetByUserId(Guid userId)
        {
            var item = await Query().FirstOrDefaultAsync(r => r.UserId == userId);
            if (item is null)
            {
                return default;
            }
            return MapEntityToDto(item);
        }
    }
}