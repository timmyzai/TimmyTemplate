
namespace ByteAwesome.Services.TestAPI
{
    public class AppException : Exception
    {
        public string StatusCode { get; set; }
        public string JsonData { get; set; }

        public AppException(string statusCode, string message, string jsonData = null) : base(message)
        {
            StatusCode = statusCode;
            JsonData = jsonData;
        }
    }
}