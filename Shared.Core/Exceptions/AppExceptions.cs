using Serilog;

namespace AwesomeProject
{
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
            if (messageTemplate == statusCode)
            {
                throw new Exception(statusCode);
            }
            if (args.Length == 0)
            {
                return messageTemplate;
            }
            try
            {
                return string.Format(messageTemplate, args);
            }
            catch (FormatException ex)
            {
                Log.Error(ex, "Error in formatting message: {0}", messageTemplate);
                return messageTemplate;
            }
        }
    }
}