using System.Security.Claims;
using ByteAwesome.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ByteAwesome
{
    public class ValidateTokenIntegrityAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()) return;
                var httpContext = context.HttpContext;
                var user = httpContext.User;
                if (!user.Identity.IsAuthenticated)
                {
                    context.Result = ContextResponseHelper.CreateUnauthorizedResponse();
                    return;
                }
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    context.Result = ContextResponseHelper.CreateUnauthorizedResponse();
                    return;
                }
                var deviceInfo = httpContext.Items["DeviceInfo"] as DeviceInfo;
                if (deviceInfo is null)
                {
                    context.Result = ContextResponseHelper.CreateUnauthorizedResponse();
                    return;
                }
                // if (GeneralHelper.IsDevelopmentEnvironment())
                // {
                //     return;
                // }
                var tokenKey = user.Claims.FirstOrDefault(x => x.Type == "UserLoginSessionId")?.Value;
                if (string.IsNullOrEmpty(tokenKey))
                {
                    context.Result = ContextResponseHelper.CreateUnauthorizedResponse();
                    return;
                }
                var redisCacheService = httpContext.RequestServices.GetRequiredService<IRedisCacheService>();
                var redisValue = await redisCacheService.GetAsync<string>(tokenKey);
                var token = httpContext.Items["DecryptedToken"] as string;
                if (string.IsNullOrEmpty(token) || redisValue != token)
                {
                    context.Result = ContextResponseHelper.CreateUnauthorizedResponse();
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Authorization failed with error: {ex.Message}", ex);
                context.Result = ContextResponseHelper.CreateUnauthorizedResponse();
            }
        }
    }
}