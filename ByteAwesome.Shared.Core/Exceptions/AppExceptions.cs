using ByteAwesome;

public class AppException : Exception
{
    public string StatusCode { get; set; }
    public string JsonData { get; set; }

    public AppException(string statusCode, params object[] args) : base(GetFormattedMessage(statusCode, args))
    {
        StatusCode = statusCode;
        JsonData = null;
    }
    public AppException(string statusCode, string jsonData, params object[] args) : base(GetFormattedMessage(statusCode, args))
    {
        StatusCode = statusCode;
        JsonData = jsonData;
    }
    private static string GetFormattedMessage(string statusCode, params object[] args)
    {
    
        string messageTemplate = LanguageService.Translate(statusCode);
        return String.Format(messageTemplate, args);
    }
}
