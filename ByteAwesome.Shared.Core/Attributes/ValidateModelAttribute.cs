using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
    public class PositiveNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            try
            {
                return value is IConvertible convertible && Convert.ToDouble(convertible, CultureInfo.InvariantCulture) > 0;
            }
            catch
            {
                return false;
            }
        }
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a positive number.";
        }
    }

}