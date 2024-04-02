namespace ByteAwesome.Services
{
    public class ResponseDto<TResult>
    {
        public bool IsSuccess { get; set; } = true;
        public TResult Result { get; set; }
        public string DisplayMessage { get; set; } = "";
        public ErrorDto Error { get; set; } = new();
    }
    public class ErrorDto
    {
        public string StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string JsonData { get; set; }

    }
}