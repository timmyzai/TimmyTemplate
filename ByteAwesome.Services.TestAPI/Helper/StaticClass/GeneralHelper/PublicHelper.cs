using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ByteAwesome.Services.TestAPI.Helper
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
            Log.Error(ex, $"{statusCode} - {logMessage}");
        }
    }
    public class GeneralHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            return attributes != null && attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
        public static string GenerateCustomGuid(string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            string result = input + Guid.NewGuid().ToString();
            return result;
        }
    }
}