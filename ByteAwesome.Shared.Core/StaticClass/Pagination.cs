using System.Text;

namespace ByteAwesome
{
    public class PagedList<TEntityDto> : List<TEntityDto>
    {
        public IEnumerable<TEntityDto> Items { get; private set; }
        public int TotalCount { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages { get; private set; }
        public string NextCursor { get; private set; }
        public string PrevCursor { get; private set; }
        public PagedList(IEnumerable<TEntityDto> items, int totalCount, int pageSize, int totalPages, string nextCursor, string prevCursor)
        {
            Items = items;
            TotalCount = totalCount;
            PageSize = pageSize;
            TotalPages = totalPages;
            NextCursor = nextCursor;
            PrevCursor = prevCursor;
        }

        public static PagedList<TEntityDto> ToPagedList(IEnumerable<TEntityDto> items, PaginationRequestDto paging)
        {
            var totalPages = (int)Math.Ceiling(paging.TotalCount / (double)paging.PageSize);
            string nextCursor = null;
            string prevCursor = null;

            if (typeof(IAuditedEntityDto).IsAssignableFrom(typeof(TEntityDto)) && items.Any())
            {
                var firstItem = items.First() as IAuditedEntityDto;
                var lastItem = items.Last() as IAuditedEntityDto;
                // Generate previous cursor if not on the first page
                prevCursor = paging.HasPreviousCursor || paging.Cursor is not null || items.Count() < paging.PageSize
                    ? CursorHelper.Encrypt(CursorConst.PrevPrefix, firstItem.CreatedTime.ToString("o"), CursorConst.PrevKey)
                    : null;

                // Generate next cursor if the page is full
                nextCursor = items.Count() == paging.PageSize
                    ? CursorHelper.Encrypt(CursorConst.NextPrefix, lastItem.CreatedTime.ToString("o"), CursorConst.NextKey)
                    : null;
            }
            return new PagedList<TEntityDto>(
                items,
                paging.TotalCount,
                paging.PageSize,
                totalPages,
                nextCursor,
                prevCursor
            );
        }
    }
    public class FilterPaginationRequestDto : PaginationRequestDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    public class PaginationRequestDto
    {
        public PaginationRequestDto() { }
        const int maxPageSize = 150;
        public string Cursor { get; set; }
        public List<SortParameter> SortParameters { get; set; }
        private int _pageSize = 50;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > maxPageSize ? maxPageSize : value;
        }
        internal bool HasPreviousCursor => Cursor is not null && Cursor.StartsWith(CursorConst.PrevPrefix);
        internal bool HasNextCursor => Cursor is not null && Cursor.StartsWith(CursorConst.NextPrefix);
        internal int TotalCount { get; private set; }
        internal void SetTotalCount(int total)
        {
            TotalCount = total;
        }
    }
    public class BO_PaginationRequestDto : PaginationRequestDto
    {
        public List<FilterParameter> FilterParameters { get; set; }
    }
    public static class CursorHelper
    {
        public static string Encrypt(string prefix, string text, string key)
        {
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(text);
            var adjusted = prefix + Convert.ToBase64String(bytesToEncode);
            return adjusted;
        }

        public static string Decrypt(string prefix, string encodedData, string key)
        {
            var adjusted = encodedData.Substring(prefix.Length);
            byte[] decodedBytes = Convert.FromBase64String(adjusted);
            return Encoding.UTF8.GetString(decodedBytes);
        }
    }

}