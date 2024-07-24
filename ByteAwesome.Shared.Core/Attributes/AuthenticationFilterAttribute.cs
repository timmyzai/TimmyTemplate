using System.Security.Claims;
using System.Text.Json;
using ByteAwesome.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StackExchange.Redis;

namespace ByteAwesome
{
    public class ValidateTokenIntegrityAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()) return;
            var httpContext = context.HttpContext;
            var user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                await ContextResponseHelper.SetUnauthorizedResponse(httpContext);
                return;
            }
            var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await ContextResponseHelper.SetUnauthorizedResponse(httpContext);
                return;
            }
            var deviceInfo = httpContext.Items["DeviceInfo"] as DeviceInfo;
            if (deviceInfo == null)
            {
                await ContextResponseHelper.SetUnauthorizedResponse(httpContext);
                return;
            }
            var tokenKey = user.Claims.FirstOrDefault(x => x.Type == "UserLoginSessionId")?.Value;
            if (string.IsNullOrEmpty(tokenKey))
            {
                await ContextResponseHelper.SetUnauthorizedResponse(httpContext);
                return;
            }
            var redisCacheService = httpContext.RequestServices.GetRequiredService<IRedisCacheService>();
            var redisValue = await redisCacheService.GetAsync(tokenKey);
            var storedToken = redisValue.ToString();
            var token = httpContext.Items["DecryptedToken"] as string;
            if (string.IsNullOrEmpty(token) || storedToken != token)
            {
                await ContextResponseHelper.SetUnauthorizedResponse(httpContext);
                return;
            }
        }
    }
}