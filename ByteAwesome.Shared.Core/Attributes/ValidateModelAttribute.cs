using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ByteAwesome
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errorMessages = context.ModelState
                    .SelectMany(ms => ms.Value.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "A validation error occurred." : e.ErrorMessage)
                    .ToArray();

                var combinedErrorMessage = string.Join("; ", errorMessages);

                var errorResponse = new ResponseDto<object>
                {
                    IsSuccess = false,
                    DisplayMessage = "Validation errors occurred.",
                    Error = new ErrorDto
                    {
                        StatusCode = ErrorCodes.General.InvalidField,
                        ErrorMessage = combinedErrorMessage
                    },
                    Result = null
                };
                context.Result = new BadRequestObjectResult(errorResponse);
            }
        }
    }
}