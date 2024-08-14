using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ByteAwesome.UserAPI.Models.Dtos;
using ByteAwesome.UserAPI.Models;
using ByteAwesome.UserAPI.DbContexts;

namespace ByteAwesome.UserAPI.Repository
{
    public class UserLoginSessionInfoRepository : BaseRepository<UserLoginSessionInfo, UserLoginSessionInfoDto, UserLoginSessionInfoDto, Guid>, IUserLoginSessionInfoRepository
    {
        public UserLoginSessionInfoRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper) { }
        public async Task<IEnumerable<UserLoginSessionInfoDto>> GetByUserId(Guid userId)
        {
            var items = await Query().Where(x => x.UsersId == userId).ToListAsync();
            if (items.Count > 0 && items.First().Users.IsDeleted)
            {
                ContextEntity().RemoveRange(items);
                await context.SaveChangesAsync();
                return new List<UserLoginSessionInfoDto>();
            }
            return MapEntitiesToDtos(items);
        }
        public async Task CleanUpOldSessions()
        {
            try
            {
                var thresholdTime = DateTime.UtcNow.AddHours(-24);
                var itemsToDelete = await base.Query(isBoRequest: true)
                                        .Where(x => x.LastLoginTime < thresholdTime)
                                        .ToListAsync();
                if (itemsToDelete.Count > 0)
                {
                    ContextEntity().RemoveRange(itemsToDelete);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to clean up old sessions");
            }
        }
        protected override IQueryable<UserLoginSessionInfo> Query(bool isBoRequest = false)
        {
            var query = base.Query(isBoRequest).Include(x => x.Users);
            return query;
        }
    }
}