namespace AwesomeProject
{
    #region ResponseDto
    public class ResponseDto<TResult>
    {
        public bool IsSuccess { get; set; } = true;
        public string DisplayMessage { get; set; } = "";
        public ErrorDto Error { get; set; } = new();
        public TResult Result { get; set; }
        public PaginationMetadata Pagination { get; set; }
        public ResponseDto()
        {
            Result = typeof(TResult).IsClass && typeof(TResult).GetConstructor(Type.EmptyTypes) is not null
                     ? (TResult)Activator.CreateInstance(typeof(TResult))
                     : default;
        }
        public void SetPaginationMetadata<T>(PagedList<T> pagedList)
        {
            if (pagedList is not null)
            {
                Pagination = new PaginationMetadata
                {
                    TotalPages = pagedList.TotalPages,
                    PageSize = pagedList.PageSize,
                    TotalCount = pagedList.TotalCount,
                    NextCursor = pagedList.NextCursor,
                    PrevCursor = pagedList.PrevCursor
                };
            }
        }
    }
    public class PaginationMetadata
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string NextCursor { get; set; }
        public string PrevCursor { get; set; }
    }
    #endregion
    #region ErrorDto
    public class ErrorDto
    {
        public string StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string JsonData { get; set; }
    }
    #endregion
}