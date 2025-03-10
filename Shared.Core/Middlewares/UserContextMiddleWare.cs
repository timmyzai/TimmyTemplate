using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AwesomeProject
{

    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    CurrentSession.SetUser(httpContext, userId, httpContext.User);
                }
            }
            await _next(httpContext);
        }
    }
}