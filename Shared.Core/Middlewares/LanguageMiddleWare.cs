using Microsoft.AspNetCore.Http;

namespace AwesomeProject
{
    public class LanguageMiddleware
    {
        private readonly RequestDelegate _next;

        public LanguageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var languageHeader = context.Request.Headers["Accept-Language"].ToString();
            var primaryLanguage = LanguageService.DeterminePrimaryLanguageCode(languageHeader);
            context.Items["UserLanguage"] = primaryLanguage;
            await _next(context);
        }
    }

}