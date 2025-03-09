using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AwesomeProject
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
        public static IActionResult CreateErrorResponse(string displayMessage, string errorMessage, string statusCode, int httpStatusCode)
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
            var result = new ObjectResult(response)
            {
                StatusCode = httpStatusCode,
                DeclaredType = typeof(ResponseDto<object>)
            };
            result.ContentTypes.Add("application/json");
            return result;
        }

        public static IActionResult CreateUnauthorizedResponse(string errorMessage = "Authentication failed. Invalid Token.")
        {
            return CreateErrorResponse("Unauthorized access.", errorMessage, ErrorCodes.General.PleaseLogin, StatusCodes.Status401Unauthorized);
        }

        public static IActionResult CreateInternalServerErrorResponse(string errorMessage = "Internal Server Error. Please try again later.")
        {
            return CreateErrorResponse("An error occurred while processing your request.", errorMessage, ErrorCodes.General.UnhandledError, StatusCodes.Status500InternalServerError);
        }

        public static async Task SetResponseToHttpContext(this HttpContext context, IActionResult actionResult)
        {
            await context.Response.WriteAsync(JsonSerializer.Serialize(actionResult));
        }
        public static async Task HandleAccessTokenDecryptionFailure(this MessageReceivedContext context, Exception ex)
        {
            Log.Error(ex, "Unauthorized Access.");
            context.Fail("Unauthorized Access.");
            await WriteUnauthorizedResponse(context);
        }
        private static async Task WriteUnauthorizedResponse(MessageReceivedContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            IActionResult actionResult = CreateUnauthorizedResponse();
            if (actionResult is ObjectResult objectResult)
            {
                var response = JsonSerializer.Serialize(objectResult.Value, new JsonSerializerOptions { WriteIndented = true });
                await context.Response.WriteAsync(response);
            }
            await context.Response.CompleteAsync();
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