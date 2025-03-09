using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserAPI.Models.Dtos;
using UserAPI.Models;
using UserAPI.DbContexts;
using AwesomeProject;

namespace UserAPI.Repository
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken, RefreshTokenDto, RefreshTokenDto, int>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }

        public override Task<RefreshTokenDto> Add(RefreshTokenDto input)
        {
            var existingTokens = ContextEntity().Where(x => x.UserId == input.UserId);
            ContextEntity().RemoveRange(existingTokens);
            return base.Add(input);
        }
        public async Task<RefreshTokenDto> GetByUserId(Guid userId)
        {
            var item = await ContextEntity().FirstOrDefaultAsync(x => x.UserId == userId);
            return mapper.Map<RefreshTokenDto>(item);
        }
        public async Task RemoveByUserId(Guid userId)
        {
            var items = await ContextEntity().Where(x => x.UserId == userId).ToListAsync();
            ContextEntity().RemoveRange(items);
        }
    }
}