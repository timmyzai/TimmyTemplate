using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ByteAwesome
{
    public class ValidateTokenIntegrityAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()) return;
            var httpContext = context.HttpContext;
            var redis = httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var deviceInfo = httpContext.Items["DeviceInfo"] as DeviceInfo;
            if (deviceInfo == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var tokenKey = user.Claims.FirstOrDefault(x => x.Type == "UserLoginSessionId")?.Value;
            var redisValue = await redis.GetDatabase().StringGetAsync(tokenKey);
            var storedToken = redisValue.ToString();
            var token = httpContext.Items["DecryptedToken"] as string;
            if (string.IsNullOrEmpty(token) || storedToken != token)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}