using System.Text.Json;
using Microsoft.AspNetCore.Http;
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
        public static void HandleException(Exception ex, ResponseDto response, string logMessage, string statusCode = ErrorCodes.General.UnhandledError, string jsonData = null)
        {
            response.IsSuccess = false;
            response.Error.StatusCode = statusCode;
            response.Error.JsonData = jsonData;
            response.Error.ErrorMessage = ex.Message;
            Log.Error(ex, logMessage);
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
    public class ContextResponseHelper
    {
        public static async Task WriteResponseAsync(HttpContext httpContext, ResponseDto response)
        {
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        public static async Task SetErrorResponse(HttpContext httpContext, string displayMessage, string errorMessage, string statusCode, int httpStatusCode)
        {
            var response = new ResponseDto
            {
                IsSuccess = false,
                DisplayMessage = displayMessage,
                Error = new ErrorDto
                {
                    StatusCode = statusCode,
                    ErrorMessage = errorMessage
                }
            };

            httpContext.Response.StatusCode = httpStatusCode;
            await WriteResponseAsync(httpContext, response);
        }

        public static async Task SetUnauthorizedResponse(HttpContext httpContext, string errorMessage = "Authentication failed. Invalid Token.")
        {
            await SetErrorResponse(httpContext, "Unauthorized access.", errorMessage, ErrorCodes.General.PleaseLogin, StatusCodes.Status401Unauthorized);
        }

        public static async Task SetInternalServerErrorResponse(HttpContext httpContext, string errorMessage = "Internal Server Error. Please try again later.")
        {
            await SetErrorResponse(httpContext, "An error occurred while processing your request.", errorMessage, ErrorCodes.General.UnhandledError, StatusCodes.Status500InternalServerError);
        }
    }
}