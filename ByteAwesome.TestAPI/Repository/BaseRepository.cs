using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq.Expressions;
using ByteAwesome.TestAPI.DbContexts;

namespace ByteAwesome.TestAPI.Repository
{
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
        #region CRUD
        public virtual async Task<TEntityDto> Add(TCreateDto input)
        {
            try
            {
                var item = mapper.Map<TEntity>(input);
                context.Entry(item).State = EntityState.Added;
                await context.SaveChangesAsync();
                return MapEntityToDto(item);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add entities");
                throw new AppException(ErrorCodes.General.EntityAdd, ex.Message);
            }
        }

        public virtual async Task<TEntityDto> Update(TEntityDto input)
        {
            try
            {
                var item = await BoQuery().FirstOrDefaultAsync(r => Equals(r.Id, input.Id));
                if (item == null)
                {
                    throw new AppException(ErrorCodes.General.EntityNameNotFound, string.Format(LanguageService.Translate(ErrorCodes.General.EntityNameNotFound), typeof(TEntity).Name));//"{typeof(TEntity).Name} not found."
                }
                mapper.Map(input, item);
                context.Entry(item).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return MapEntityToDto(item);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update entities");
                throw new AppException(ErrorCodes.General.EntityUpdate, ex.Message);
            }
        }

        public virtual async Task<TEntityDto> Delete(TKey id)
        {
            try
            {
                var item = await BoQuery().FirstOrDefaultAsync(r => Equals(r.Id, id));
                if (item == null)
                {
                    throw new AppException(ErrorCodes.General.EntityNameNotFound, string.Format(LanguageService.Translate(ErrorCodes.General.EntityNameNotFound), typeof(TEntity).Name));
                }
                if (item is IFullyAuditedEntity fullyAuditedEntity && fullyAuditedEntity.IsDeleted)
                {
                    throw new AppException(ErrorCodes.General.EntityAlreadyDeleted, string.Format(LanguageService.Translate(ErrorCodes.General.EntityAlreadyDeleted), typeof(TEntity).Name));
                }
                if (item is IFullyAuditedEntity _item)
                {
                    context.Entry(_item).State = EntityState.Modified;
                    _item.IsDeleted = true;
                }
                else
                {
                    EntityContext().Remove(item);
                }
                await context.SaveChangesAsync();
                return MapEntityToDto(item);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete entities");
                throw new AppException(ErrorCodes.General.EntityDelete, ex.Message);
            }
        }
        public virtual async Task<TEntityDto> GetById(TKey id)
        {
            try
            {
                var item = await BoQuery().AsNoTracking().FirstOrDefaultAsync(r => Equals(r.Id, id));
                if (item == null)
                {
                    return default;
                }
                return MapEntityToDto(item);
            }
            catch (Exception ex)
            {
                throw new AppException(ErrorCodes.General.EntityGetById, ex.Message);
            }
        }
        public virtual async Task<IEnumerable<TEntityDto>> Get(IPagedResultRequestDto filter = null, string propertyName = null)
        {
            try
            {
                var query = BoQuery().AsNoTracking();
                if (propertyName != null)
                {
                    query = Sort(query, EntitySortOrder.Asc, propertyName);
                }
                var items = await query.ToListAsync();
                return MapEntitiesToDtos(items);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get all entities");
                throw new AppException(ErrorCodes.General.EntityGetAll, ex.Message);
            }
        }
        public virtual async Task<IEnumerable<TEntityDto>> GetDesc(IPagedResultRequestDto filter = null, string propertyName = null)
        {
            try
            {
                var query = BoQuery().AsNoTracking();
                query = Sort(query, EntitySortOrder.Desc);
                var items = await query.ToListAsync();
                return MapEntitiesToDtos(items);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get all entities");
                throw new AppException(ErrorCodes.General.EntityGetAll, ex.Message);
            }
        }
        #endregion
        #region CRUD Range
        public virtual async Task<IEnumerable<TEntityDto>> AddRange(IEnumerable<TCreateDto> inputs)
        {
            try
            {
                var entities = mapper.Map<IEnumerable<TEntity>>(inputs);
                context.AddRange(entities);
                await context.SaveChangesAsync();
                return mapper.Map<IEnumerable<TEntityDto>>(entities);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add entities");
                throw new AppException(ErrorCodes.General.EntityAdd, ex.Message);
            }
        }

        public virtual async Task<IEnumerable<TEntityDto>> UpdateRange(IEnumerable<TEntityDto> inputs)
        {
            try
            {
                var ids = inputs.Select(dto => dto.Id).ToList();
                var itemsToUpdate = await BoQuery().Where(entity => ids.Contains(entity.Id)).ToListAsync();
                if (itemsToUpdate.Count != inputs.Count())
                {
                    throw new Exception("Mismatch between input count and found entities");
                }

                mapper.Map<List<TEntityDto>, List<TEntity>>(inputs.ToList(), itemsToUpdate);
                context.UpdateRange(itemsToUpdate);
                await context.SaveChangesAsync();
                return mapper.Map<IEnumerable<TEntityDto>>(itemsToUpdate);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update entities");
                throw new AppException(ErrorCodes.General.EntityUpdate, ex.Message);
            }
        }
        public virtual async Task<IEnumerable<TEntityDto>> DeleteRange(IEnumerable<TKey> ids)
        {
            try
            {
                var itemsToDelete = await BoQuery().Where(entity => ids.Contains(entity.Id)).ToListAsync();
                if (typeof(IFullyAuditedEntity).IsAssignableFrom(typeof(TEntity)))
                {
                    foreach (var item in itemsToDelete.Cast<IFullyAuditedEntity>())
                    {
                        item.IsDeleted = true;
                        context.Entry(item).State = EntityState.Modified;
                    }
                }
                else
                {
                    context.RemoveRange(itemsToDelete);
                }
                await context.SaveChangesAsync();
                return mapper.Map<IEnumerable<TEntityDto>>(itemsToDelete);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete entities");
                throw new AppException(ErrorCodes.General.EntityDelete, ex.Message);
            }
        }
        #endregion
        #region Convertors
        public TEntityDto MapEntityToDto(TEntity item)
        {
            return mapper.Map<TEntityDto>(item);
        }
        public IEnumerable<TEntityDto> MapEntitiesToDtos(List<TEntity> items)
        {
            return mapper.Map<IEnumerable<TEntityDto>>(items);
        }
        public PagedList<Dtos> MapDtosToPagedList<Dtos>(IEnumerable<TEntityDto> items, int pageNumber, int pageSize)
        {
            var dtos = mapper.Map<IEnumerable<Dtos>>(items);
            return PagedList<Dtos>.ToPagedList(dtos, pageNumber, pageSize);
        }
        public PagedList<TEntityDto> MapDtosToPagedList(IEnumerable<TEntityDto> items, int pageNumber, int pageSize)
        {
            return PagedList<TEntityDto>.ToPagedList(items, pageNumber, pageSize);
        }
        public IQueryable<TEntity> Sort(IQueryable<TEntity> items, EntitySortOrder order = EntitySortOrder.Desc, string propertyName = "CreatedTime")
        {
            var keySelector = CreateKeySelector(propertyName);
            return order == EntitySortOrder.Asc ? items.OrderBy(keySelector) : items.OrderByDescending(keySelector);
        }
        #endregion
        #region Queries
        public DbSet<TEntity> EntityContext()
        {
            return context.Set<TEntity>();
        }
        public IQueryable<TEntity> BoQuery()
        {
            return EntityContext().AsQueryable();
        }
        public IQueryable<TEntity> Query()
        {
            var query = EntityContext().AsQueryable();
            if (typeof(IFullyAuditedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(x => !((IFullyAuditedEntity)x).IsDeleted);
            }
            return query;
        }
        private Expression<Func<TEntity, object>> CreateKeySelector(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.PropertyOrField(parameter, propertyName);
            var convert = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<TEntity, object>>(convert, parameter);
        }

        #endregion
    }
}
