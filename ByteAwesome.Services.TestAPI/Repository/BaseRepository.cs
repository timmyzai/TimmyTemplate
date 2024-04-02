using AutoMapper;
using ByteAwesome.Services.TestAPI.DbContexts;
using ByteAwesome.Services.Entities;
using Microsoft.EntityFrameworkCore;
using ByteAwesome.Services.EntitiesDto;
using Serilog;

namespace ByteAwesome.Services.TestAPI.Repository
{
    public interface IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        Task<TEntityDto> Add(TCreateDto input);
        Task<TEntityDto> Delete(TKey id);
        Task<IEnumerable<TEntityDto>> Get();
        Task<IEnumerable<TEntityDto>> GetDesc();
        Task<TEntityDto> GetById(TKey id);
        Task<TEntityDto> Update(TEntityDto input);
    }

    public abstract class BaseRepository<TEntity, TEntityDto, TCreateDto, TKey> : IBaseRepository<TEntityDto, TCreateDto, TKey>
        where TEntity : class, IEntity<TKey>
        where TEntityDto : IEntityDto<TKey>
    {
        protected readonly ApplicationDbContext context;
        protected readonly IMapper mapper;

        protected BaseRepository(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public virtual async Task<TEntityDto> Add(TCreateDto input)
        {
            try
            {
                var item = mapper.Map<TEntity>(input);
                context.Entry(item).State = EntityState.Added;
                await context.SaveChangesAsync();
                return mapper.Map<TEntityDto>(item);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ErrorCodes.General.EntityAdd} - {LanguageService.Translate(ErrorCodes.General.EntityAdd)}");
                throw new AppException(ErrorCodes.General.EntityAdd, ex.Message);
            }
        }

        public virtual async Task<TEntityDto> Update(TEntityDto input)
        {
            try
            {
                var item = await context.Set<TEntity>().FirstOrDefaultAsync(r => Equals(r.Id, input.Id));
                if (item == null || (item is IFullyAuditedEntity fullyAuditedEntity && fullyAuditedEntity.IsDeleted))
                {
                    throw new Exception($"{typeof(TEntity).Name} not found");
                }
                mapper.Map(input, item);
                context.Entry(item).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return mapper.Map<TEntityDto>(item);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ErrorCodes.General.EntityUpdate} - {LanguageService.Translate(ErrorCodes.General.EntityUpdate)}");
                throw new AppException(ErrorCodes.General.EntityUpdate, ex.Message);
            }
        }

        public virtual async Task<TEntityDto> Delete(TKey id)
        {
            try
            {
                var item = await context.Set<TEntity>().FirstOrDefaultAsync(r => Equals(r.Id, id));
                if (item == null || (item is IFullyAuditedEntity fullyAuditedEntity && fullyAuditedEntity.IsDeleted))
                {
                    throw new Exception($"{typeof(TEntity).Name} not found");
                }
                if (item is IFullyAuditedEntity _item)
                {
                    context.Entry(_item).State = EntityState.Modified;
                    _item.IsDeleted = true;
                    await context.SaveChangesAsync();
                    return mapper.Map<TEntityDto>(item);
                }
                else
                {
                    context.Set<TEntity>().Remove(item);
                    await context.SaveChangesAsync();
                    return mapper.Map<TEntityDto>(item);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ErrorCodes.General.EntityDelete} - {LanguageService.Translate(ErrorCodes.General.EntityDelete)}");
                throw new AppException(ErrorCodes.General.EntityDelete, ex.Message);
            }
        }
        public virtual async Task<TEntityDto> GetById(TKey id)
        {
            try
            {
                var item = await context.Set<TEntity>().FirstOrDefaultAsync(r => Equals(r.Id, id));
                if (item == null || (item is IFullyAuditedEntity fullyAuditedEntity && fullyAuditedEntity.IsDeleted))
                {
                    return default;
                }
                return mapper.Map<TEntityDto>(item);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ErrorCodes.General.EntityGetById} - {string.Format(LanguageService.Translate(ErrorCodes.General.EntityGetById),id)})");
                throw new AppException(ErrorCodes.General.EntityGetById, ex.Message);
            }
        }
        public virtual async Task<IEnumerable<TEntityDto>> Get()
        {
            try
            {
                var items = await context.Set<TEntity>().ToListAsync();
                return MapAndFilterItems(items);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ErrorCodes.General.EntityGetAll} - {LanguageService.Translate(ErrorCodes.General.EntityGetAll)}");
                throw new AppException(ErrorCodes.General.EntityGetAll, ex.Message);
            }
        }
        public virtual async Task<IEnumerable<TEntityDto>> GetDesc()
        {
            try
            {
                var items = await context.Set<TEntity>().ToListAsync();
                return MapAndFilterItemsDesc(items);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{ErrorCodes.General.EntityGetAll} - {LanguageService.Translate(ErrorCodes.General.EntityGetAll)}");
                throw new AppException(ErrorCodes.General.EntityGetAll, ex.Message);
            }
        }
        public IEnumerable<TEntityDto> MapAndFilterItems(List<TEntity> items)
        {
            var filteredItems = items.Where(item => !(item is IFullyAuditedEntity fullyAuditedEntity && fullyAuditedEntity.IsDeleted));
            return mapper.Map<IEnumerable<TEntityDto>>(filteredItems);
        }
        public IEnumerable<TEntityDto> MapAndFilterItemsDesc(List<TEntity> items)
        {
            var filteredItems = items.Where(item => !(item is IFullyAuditedEntity fullyAuditedEntity && fullyAuditedEntity.IsDeleted));
            var auditedItems = filteredItems.OfType<IAuditedEntity>();
            if (auditedItems.Any())
            {
                filteredItems = filteredItems.OrderByDescending(x => auditedItems.Contains(x as IAuditedEntity) ? ((IAuditedEntity)x).CreatedTime : default);
            }
            return mapper.Map<IEnumerable<TEntityDto>>(filteredItems);
        }
    }
}
