using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace ByteAwesome
{
    public interface IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        Task<TEntityDto> Add(TCreateDto input);
        Task<TEntityDto> Update(TEntityDto input);
        Task<TEntityDto> Delete(TKey id);
        Task<TEntityDto> GetById(TKey id, bool isBoRequest = false);
        Task<IEnumerable<TEntityDto>> Get(PaginationRequestDto pagingFilter = null, bool isBoRequest = false);
        Task<IEnumerable<TEntityDto>> AddRange(IEnumerable<TCreateDto> inputs);
        Task<IEnumerable<TEntityDto>> UpdateRange(IEnumerable<TEntityDto> inputs);
        Task<IEnumerable<TEntityDto>> DeleteRange(IEnumerable<TKey> ids);
        Task<IEnumerable<TEntityDto>> UpsertRange(IEnumerable<TEntityDto> entities, params Expression<Func<TEntityDto, object>>[] identifierExpressions);
    }
    public abstract class BaseRepository<TEntity, TEntityDto, TCreateDto, TKey> : IBaseRepository<TEntityDto, TCreateDto, TKey>
        where TEntity : class, IEntity<TKey>
        where TEntityDto : IEntityDto<TKey>
    {
        protected readonly DbContext context;
        protected readonly IMapper mapper;

        protected BaseRepository(DbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        #region CRUD
        public virtual async Task<TEntityDto> Add(TCreateDto input)
        {
            var item = mapper.Map<TEntity>(input);
            context.Add(item);
            await context.SaveChangesAsync();
            return MapEntityToDto(item);
        }
        public virtual async Task<TEntityDto> Update(TEntityDto input)
        {
            var item = await ContextEntity().FindAsync(input.Id);
            if (item is null)
            {
                throw new AppException(ErrorCodes.General.EntityNameNotFound, args: typeof(TEntity).Name);
            }
            mapper.Map(input, item);
            await context.SaveChangesAsync();
            return mapper.Map<TEntityDto>(item); //make sure it is not overriden
        }
        public virtual async Task<TEntityDto> Delete(TKey id)
        {
            var item = await ContextEntity().FindAsync(id);
            if (item is null)
            {
                throw new AppException(ErrorCodes.General.EntityNameNotFound, args: typeof(TEntity).Name);
            }
            MarkAsDeleted(item);
            await context.SaveChangesAsync();
            return mapper.Map<TEntityDto>(item); //make sure it is not overriden
        }
        #endregion
        #region CRUD Range
        public virtual async Task<IEnumerable<TEntityDto>> AddRange(IEnumerable<TCreateDto> inputs)
        {
            var results = new List<TEntity>();
            await ProcessInBatches(inputs, async (batch) =>
            {
                var entities = mapper.Map<IEnumerable<TEntity>>(batch);
                context.AddRange(entities);
                await context.SaveChangesAsync();
                results.AddRange(entities);
            });
            return MapEntitiesToDtos(results);
        }
        public virtual async Task<IEnumerable<TEntityDto>> UpdateRange(IEnumerable<TEntityDto> entities)
        {
            var ids = entities.Select(e => e.Id).ToList();
            var existingEntitiesDict = await GetExistingEntitiesWithCheck(ids);

            var results = new List<TEntity>();
            await ProcessInBatches(entities, async (batch) =>
            {
                foreach (var entityDto in batch)
                {
                    if (existingEntitiesDict.TryGetValue(entityDto.Id, out var existingEntity))
                    {
                        var updatedEntity = mapper.Map(entityDto, existingEntity);
                        results.Add(updatedEntity);
                    }
                }
                await context.SaveChangesAsync();
            });
            return mapper.Map<IEnumerable<TEntityDto>>(entities); //make sure it is not overriden
        }
        public async Task<IEnumerable<TEntityDto>> UpsertRange(IEnumerable<TEntityDto> entities, params Expression<Func<TEntityDto, object>>[] identifierExpressions)
        {
            if (entities is null || !entities.Any()) throw new ArgumentNullException(nameof(entities));
            if (identifierExpressions is null || !identifierExpressions.Any()) throw new ArgumentException("Identifier expressions are required for upsert operation.");

            var keySelector = CompileKeySelector(identifierExpressions);
            CheckForDuplicates(entities, keySelector);

            var allEntities = await context.Set<TEntity>().ToListAsync();
            var existingEntitiesDict = allEntities.ToDictionary(e => keySelector(mapper.Map<TEntityDto>(e)));

            var updateList = new List<TEntity>();
            var addList = new List<TEntity>();
            var results = new List<TEntityDto>();
            foreach (var entityDto in entities)
            {
                var key = keySelector(entityDto);
                if (existingEntitiesDict.TryGetValue(key, out var existingEntity))
                {
                    mapper.Map(entityDto, existingEntity);
                    updateList.Add(existingEntity);
                }
                else
                {
                    var newEntity = mapper.Map<TEntity>(entityDto);
                    addList.Add(newEntity);
                }
            }
            await ProcessInBatches(addList, async (batch) =>
            {
                await context.AddRangeAsync(batch);
                await context.SaveChangesAsync();
                results.AddRange(mapper.Map<IEnumerable<TEntityDto>>(batch));
            });
            await ProcessInBatches(updateList, async (batch) =>
            {
                await context.SaveChangesAsync();
                results.AddRange(mapper.Map<IEnumerable<TEntityDto>>(batch));
            });
            await context.SaveChangesAsync();
            return mapper.Map<IEnumerable<TEntityDto>>(entities); //make sure it is not overriden
        }
        public virtual async Task<IEnumerable<TEntityDto>> DeleteRange(IEnumerable<TKey> ids)
        {
            var existingEntitiesDict = await GetExistingEntitiesWithCheck(ids);
            var itemsToDelete = existingEntitiesDict.Values.ToList();

            var results = new List<TEntity>();

            await ProcessInBatches(itemsToDelete, async (batch) =>
            {
                MarkAsDeleted(batch);
                await context.SaveChangesAsync();
                results.AddRange(batch);
            });
            return mapper.Map<IEnumerable<TEntityDto>>(results); //make sure it is not overriden
        }
        #endregion
        #region Queries
        public virtual async Task<TEntityDto> GetById(TKey id, bool isBoRequest = false)
        {
            var item = await Query(isBoRequest: isBoRequest).FirstOrDefaultAsync(r => Equals(r.Id, id));
            if (item is null)
            {
                return default;
            }
            return MapEntityToDto(item);
        }
        public virtual async Task<IEnumerable<TEntityDto>> Get(PaginationRequestDto pagingFilter = null, bool isBoRequest = false)
        {
            var query = Query(isBoRequest);
            query = CursorPagination(query, pagingFilter);
            var items = await query.ToListAsync();
            return MapEntitiesToDtos(items);
        }
        #endregion
        #region Contexts
        protected virtual DbSet<TEntity> ContextEntity() // without joint tables
        {
            return context.Set<TEntity>();
        }
        protected virtual DbSet<T> ContextEntity<T>() where T : class
        {
            return context.Set<T>();
        }
        protected virtual IQueryable<TEntity> Query(bool isBoRequest = false)
        {
            var query = ContextEntity().AsNoTracking().AsQueryable();
            if (isBoRequest)
            {
                query = query.IgnoreQueryFilters();
            }
            else if (UserContext.CurrentUserId.HasValue && typeof(IUserIdEntity).IsAssignableFrom(typeof(TEntity)))
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var property = Expression.Property(parameter, "UserId");
                var value = Expression.Constant(UserContext.CurrentUserId.Value, typeof(Guid));
                var equals = Expression.Equal(property, value);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

                query = query.Where(lambda);
            }
            return query;
        }
        protected virtual IQueryable<TEntity> CursorPagination(IQueryable<TEntity> query, PaginationRequestDto pagingFilter = null)
        {
            pagingFilter ??= new PaginationRequestDto();
            pagingFilter.SetTotalCount(query.Count());
            if (typeof(IAuditedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                if (pagingFilter.HasNextCursor)
                {
                    var cursor = CursorHelper.Decrypt(CursorConst.NextPrefix, pagingFilter.Cursor, CursorConst.NextKey);
                    if (DateTime.TryParse(cursor, out DateTime cursorTime))
                    {
                        query = query.Where(x => (x as IAuditedEntity).CreatedTime < cursorTime);
                    }
                }
                else if (pagingFilter.HasPreviousCursor)
                {
                    var cursor = CursorHelper.Decrypt(CursorConst.PrevPrefix, pagingFilter.Cursor, CursorConst.PrevKey);
                    if (DateTime.TryParse(cursor, out DateTime cursorTime))
                    {
                        query = query.Where(x => (x as IAuditedEntity).CreatedTime > cursorTime);
                        query = query.Take(pagingFilter.PageSize);
                        return Sort(query, pagingFilter.SortParameters);
                    }
                }
            }
            query = Sort(query, pagingFilter.SortParameters);
            return query.Take(pagingFilter.PageSize);
        }
        #endregion
        #region Convertors
        protected virtual TEntityDto MapEntityToDto(TEntity item)
        {
            return mapper.Map<TEntityDto>(item);
        }
        protected virtual IEnumerable<TEntityDto> MapEntitiesToDtos(IEnumerable<TEntity> entities)
        {
            return mapper.Map<IEnumerable<TEntityDto>>(entities);
        }
        protected virtual IEnumerable<T> MapEntitiesToDtos<T>(IEnumerable<TEntity> entities)
        {
            return mapper.Map<IEnumerable<T>>(entities);
        }
        protected IQueryable<TEntity> Filter(IQueryable<TEntity> query, IEnumerable<FilterParameter> filterParameters)
        {
            if (filterParameters is null) return query;

            foreach (var param in filterParameters)
            {
                if (!PropertyExists(param.PropertyName))
                {
                    throw new ArgumentException($"Property '{param.PropertyName}' does not exist on {typeof(TEntity).Name}");
                }

                var parameterExpression = Expression.Parameter(typeof(TEntity), "x");
                var propertyExpression = Expression.Property(parameterExpression, param.PropertyName);
                var constantExpression = Expression.Constant(param.Value);

                Expression comparison = param.Operator switch
                {
                    FilterOperator.Equal => Expression.Equal(propertyExpression, constantExpression),
                    FilterOperator.NotEqual => Expression.NotEqual(propertyExpression, constantExpression),
                    FilterOperator.GreaterThan => Expression.GreaterThan(propertyExpression, constantExpression),
                    FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(propertyExpression, constantExpression),
                    FilterOperator.LessThan => Expression.LessThan(propertyExpression, constantExpression),
                    FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(propertyExpression, constantExpression),
                    FilterOperator.Contains => Expression.Call(propertyExpression, "Contains", null, constantExpression),
                    FilterOperator.StartsWith => Expression.Call(propertyExpression, "StartsWith", null, constantExpression),
                    FilterOperator.EndsWith => Expression.Call(propertyExpression, "EndsWith", null, constantExpression),
                    FilterOperator.In => Expression.Call(Expression.Constant(param.Value), typeof(IEnumerable<>).MakeGenericType(propertyExpression.Type).GetMethod("Contains", new[] { propertyExpression.Type }), propertyExpression),
                    FilterOperator.NotIn => Expression.Not(Expression.Call(Expression.Constant(param.Value), typeof(IEnumerable<>).MakeGenericType(propertyExpression.Type).GetMethod("Contains", new[] { propertyExpression.Type }), propertyExpression)),
                    _ => throw new ArgumentException($"Filter operator '{param.Operator}' is not supported.")
                };

                var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, parameterExpression);
                query = query.Where(lambda);
            }

            return query;
        }
        protected IQueryable<TEntity> Sort(IQueryable<TEntity> query, IEnumerable<SortParameter> sortParameters)
        {
            if (sortParameters is null) return query;
            foreach (var param in sortParameters)
            {
                if (!PropertyExists(param.PropertyName))
                {
                    throw new ArgumentException($"Property '{param.PropertyName}' does not exist on {typeof(TEntity).Name}");
                }
                var lambda = PropertySelector<TEntity>.GetKeySelector(param.PropertyName);
                query = param.Order == EntitySortOrder.Asc ? query.OrderBy(lambda) : query.OrderByDescending(lambda);
            }
            return query;
        }
        #endregion
        #region Helpers
        private async Task<Dictionary<TKey, TEntity>> GetExistingEntitiesWithCheck(IEnumerable<TKey> ids)
        {
            var existingEntities = await ContextEntity().Where(e => ids.Contains(e.Id)).ToListAsync();
            var existingEntitiesDict = existingEntities.ToDictionary(e => e.Id);
            var missingIds = ids.Except(existingEntitiesDict.Keys).ToList();

            if (missingIds.Any())
            {
                throw new AppException(ErrorCodes.General.EntitiesNotFound, $"Entities with IDs '{string.Join(", ", missingIds)}' not found.");
            }

            return existingEntitiesDict;
        }
        private void MarkAsDeleted(IEnumerable<TEntity> entities)
        {
            var entitiesToRemove = new List<TEntity>();
            foreach (var entity in entities)
            {
                if (!CheckAndMarkEntityAsDeleted(entity))
                {
                    entitiesToRemove.Add(entity);
                }
            }
            if (entitiesToRemove.Any())
            {
                context.RemoveRange(entitiesToRemove);
            }
        }
        private void MarkAsDeleted(TEntity entity)
        {
            if (!CheckAndMarkEntityAsDeleted(entity))
            {
                context.Remove(entity);
            }
        }
        private bool CheckAndMarkEntityAsDeleted(TEntity entity)
        {
            if (entity is IFullyAuditedEntity fullyAuditedEntity)
            {
                if (fullyAuditedEntity.IsDeleted)
                {
                    throw new AppException(ErrorCodes.General.EntityAlreadyDeleted, args: typeof(TEntity).Name);
                }
                fullyAuditedEntity.IsDeleted = true;
                return true;
            }
            return false;
        }
        private async Task ProcessInBatches<T>(IEnumerable<T> entities, Func<IEnumerable<T>, Task> processBatch, int batchSize = 100)
        {
            var batches = entities.Select((item, index) => new { item, index })
                               .GroupBy(x => x.index / batchSize)
                               .Select(g => g.Select(x => x.item));

            foreach (var batch in batches)
            {
                await processBatch(batch);
            }
        }
        private void CheckForDuplicates(IEnumerable<TEntityDto> entities, Func<TEntityDto, string> keySelector)
        {
            var keys = new HashSet<string>();
            var duplicates = new HashSet<string>();
            foreach (var entity in entities)
            {
                var key = keySelector(entity);
                if (!keys.Add(key))
                    duplicates.Add(key);
            }
            if (duplicates.Any())
            {
                throw new AppException(ErrorCodes.General.EntityDuplicate,
                    $"Duplicate entities found based on provided identifiers for type {typeof(TEntity).Name}. Duplicate keys: {string.Join(", ", duplicates)}");
            }
        }
        private Func<TEntityDto, string> CompileKeySelector(params Expression<Func<TEntityDto, object>>[] identifierExpressions)
        {
            var compiled = identifierExpressions.Select(expr => expr.Compile()).ToList();
            return entity => string.Join("|", compiled.Select(id => id(entity)?.ToString() ?? string.Empty));
        }
        private bool PropertyExists(string propertyName)
        {
            return typeof(TEntity).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) is not null;
        }
        #endregion
    }
}
