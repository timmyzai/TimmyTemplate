using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ByteAwesome
{
    public static class ModelValidator
    {
        public static ActionResult Validate<T>(T model)
        {
            ValidationContext context = new ValidationContext(model);
            List<ValidationResult> results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, context, results, true);

            if (!isValid)
            {
                var combinedErrorMessage = string.Join("; ", results.Select(r => r.ErrorMessage));
                var errorResponse = new ResponseDto<object>
                {
                    IsSuccess = false,
                    DisplayMessage = "Validation errors occurred.",
                    Error = new ErrorDto
                    {
                        StatusCode = "General.InvalidField", // Modify as necessary
                        ErrorMessage = combinedErrorMessage
                    },
                    Result = null
                };
                return new BadRequestObjectResult(errorResponse);
            }
            return null;
        }
    }
}