namespace ByteAwesome
{
    public interface IBaseRepository<TEntityDto, TCreateDto, TKey>
    {
        Task<TEntityDto> Add(TCreateDto input);
        Task<IEnumerable<TEntityDto>> AddRange(IEnumerable<TCreateDto> inputs);
        Task<TEntityDto> Delete(TKey id);
        Task<IEnumerable<TEntityDto>> DeleteRange(IEnumerable<TKey> ids);
        Task<IEnumerable<TEntityDto>> Get(IPagedResultRequestDto filter = null, string propertyName = null);
        Task<TEntityDto> GetById(TKey id);
        Task<IEnumerable<TEntityDto>> GetDesc(IPagedResultRequestDto filter = null, string propertyName = null);
        Task<TEntityDto> Update(TEntityDto input);
        Task<IEnumerable<TEntityDto>> UpdateRange(IEnumerable<TEntityDto> inputs);
        PagedList<Dtos> MapDtosToPagedList<Dtos>(IEnumerable<TEntityDto> items, int pageNumber, int pageSize);
        PagedList<TEntityDto> MapDtosToPagedList(IEnumerable<TEntityDto> items, int pageNumber, int pageSize);
    }
}
