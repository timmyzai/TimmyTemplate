using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace ByteAwesome
{
    public static class ActionResultHandler
    {
        public static void HandleException<T>(
            Exception ex,
            ResponseDto<T> response,
            string errorMessage = "Error",
            string statusCode = null,
            string jsonData = null,
            object userInput = null)
        {
            if (response is not null)
            {
                response.IsSuccess = false;
                response.Result = default;
                response.Error.StatusCode = statusCode ?? ErrorCodes.General.UnhandledError;
                response.Error.JsonData = jsonData;
                response.Error.ErrorMessage = statusCode is not null && errorMessage == "Error" ? LanguageService.Translate(statusCode) : errorMessage;
            }
            LogError(ex, userInput);
        }
        public static void HandleException(
            Exception ex,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(filePath);
            string errMsg = $"{className} - {methodName} - ";
            Log.Error(ex, errMsg);
        }
        private static void LogError(Exception ex, object userInput)
        {
            var username = CurrentSession.GetUserName();
            string userInputJson = userInput is not null ? Newtonsoft.Json.JsonConvert.SerializeObject(userInput) : null;
            Log.Error(ex, string.IsNullOrEmpty(username) ? ex.Message : $"User: {username}, UserInputs: {userInputJson}");
        }

    }
    public static class ContextResponseHelper
    {
        public static async Task SetErrorResponse(HttpContext httpContext, string displayMessage, string errorMessage, string statusCode, int httpStatusCode)
        {
            var response = new ResponseDto<object>
            {
                IsSuccess = false,
                DisplayMessage = displayMessage,
                Error = new ErrorDto
                {
                    StatusCode = statusCode,
                    ErrorMessage = errorMessage
                },
                Result = null
            };
            httpContext.Response.StatusCode = httpStatusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
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
    public static class GrpcResponseValidator
    {
        public static void ValidateResponse(string errorMessage, bool fireAndForget = false)
        {
            if (!string.IsNullOrEmpty(errorMessage) && !fireAndForget) throw new AppException(errorMessage);
            else if (!string.IsNullOrEmpty(errorMessage)) Log.Error(errorMessage);
        }
    }
}