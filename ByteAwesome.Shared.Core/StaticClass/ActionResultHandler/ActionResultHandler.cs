using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ByteAwesome
{
    public class ActionResultHandler
    {
        public static TObject Process<TObject>(object jsonResult)
        {
            if (!(jsonResult is JsonResult { Value: ResponseDto<TObject> responseDto }))
            {
                throw new ArgumentException("Invalid response format. Check TObject");
            }
            if (!responseDto.IsSuccess)
            {
                throw new InvalidOperationException(responseDto.Error.ErrorMessage);
            }
            if (responseDto.Result is TObject classResult)
            {
                return classResult;
            }
            throw new InvalidOperationException("Cannot convert to TObject");
        }
        public static void HandleException<T>(Exception ex, ResponseDto<T> response, string logMessage, string statusCode = ErrorCodes.General.UnhandledError, string jsonData = null)
        {
            response.IsSuccess = false;
            response.Error.StatusCode = statusCode;
            response.Error.JsonData = jsonData;
            response.Error.ErrorMessage = ex.Message;
            Log.Error(ex, logMessage);
        }
    }
}