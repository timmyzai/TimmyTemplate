using System.Text.Json.Serialization;

namespace ByteAwesome
{
    public class PagedList<TEntityDto> : List<TEntityDto>
    {
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public PagedList() { }
        public PagedList(List<TEntityDto> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }
        public static PagedList<TEntityDto> ToPagedList(IEnumerable<TEntityDto> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<TEntityDto>(items, count, pageNumber, pageSize);
        }
    }

    public interface IPagedResultRequestDto
    {
        string Cursor { get; set; }
        int PageNumber { get; set; }
        bool IsUnlimited { get; set; }
        int PageSize { get; set; }
    }

    public class PagedResultRequestDto : IPagedResultRequestDto
    {
        const int maxPageSize = 150;
        public string Cursor { get; set; } = null; // Cursor can be a GUID, timestamp, or any other sortable field
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 100;
        [JsonIgnore]
        public bool IsUnlimited { get; set; } = false;

        public int PageSize
        {
            get
            {
                return IsUnlimited ? int.MaxValue : _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }

    public interface IAllPagedRequestDto : IPagedResultRequestDto
    {
        string Keyword { get; set; }
        bool? IsActive { get; set; }
        DateTime? FromDate { get; set; }
        DateTime? ToDate { get; set; }
    }

    public class AllPagedRequestDto : PagedResultRequestDto, IAllPagedRequestDto
    {
        public string? Keyword { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}