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
                context.Result = new OkObjectResult(errorResponse);
            }
        }
    }
    public class InjectFromHeaderAttribute : ActionFilterAttribute
    {
        public List<HeaderRequirement> headers = [];
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var header in headers)
            {
                if (context.HttpContext.Request.Headers.TryGetValue(header.Name, out var value))
                {
                    context.HttpContext.Items[header.Name] = value.ToString();
                }
                else if (header.IsRequired)
                {
                    var errorResponse = new ResponseDto<object>
                    {
                        IsSuccess = false,
                        DisplayMessage = "Validation errors occurred.",
                        Error = new ErrorDto
                        {
                            StatusCode = ErrorCodes.General.InvalidField,
                            ErrorMessage = $"Header {header.Name} is required but was not provided."
                        },
                        Result = null
                    };
                    context.Result = new OkObjectResult(errorResponse);
                    return; // Stop further execution as a required header is missing
                }
            }
        }
        public class HeaderRequirement
        {
            public string Name { get; }
            public bool IsRequired { get; }

            public HeaderRequirement(string name, bool isRequired)
            {
                Name = name;
                IsRequired = isRequired;
            }
        }
        public InjectFromHeaderAttribute(string headerName, bool isRequired = false)
        {
            headers.Add(new HeaderRequirement(headerName, isRequired));
        }
    }
}