using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ByteAwesome
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
            try
            {
                var _claim = httpContext?.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier);
                if (_claim is not null && Guid.TryParse(_claim.Value, out Guid userId) && userId != Guid.Empty)
                {
                    UserContext.CurrentUserId = userId;
                }
                await _next(httpContext);
            }
            finally
            {
                UserContext.CurrentUserId = null;
            }
        }
    }
}