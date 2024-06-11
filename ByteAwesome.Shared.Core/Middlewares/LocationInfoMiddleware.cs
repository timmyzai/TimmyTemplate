using ByteAwesome.Services;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace ByteAwesome
{
    public class LocationInfoMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IGeoIPService geoIPService;
        private readonly List<string> blacklistIpAddresses = new List<string>();
        public LocationInfoMiddleware(RequestDelegate next, IGeoIPService geoIPService)
        {
            this.next = next;
            this.geoIPService = geoIPService;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                var clientIpInfo = ExtractRealAndForwardedIp(context);
                if (clientIpInfo == null)
                {
                    context.Response.StatusCode = 403;
                    return;
                }

                var location = geoIPService.GetLocationFromIp(clientIpInfo);
                context.Items["GeoLocation"] = location;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing IP information");
            }
            await next(context);
        }

        private ClientIpInfo ExtractRealAndForwardedIp(HttpContext context)
        {
            var ipHeaders = new List<string> { "X-Forwarded-For", "X-Real-IP", "X-Client-IP", "HTTP_X_FORWARDED_FOR", "Via", "Connection" };
            var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString();
            foreach (var header in ipHeaders)
            {
                var headerValue = context.Request.Headers[header].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(headerValue) && !IsBlackListed(headerValue))
                {
                    return new ClientIpInfo
                    {
                        IpAddress = headerValue,
                        ProxyIpAddress = remoteIpAddress
                    };
                }
            }
            if (IsBlackListed(remoteIpAddress))
            {
                Log.Information("Blacklisted IP access attempt: {IP}", remoteIpAddress);
                return null;
            }
            return new ClientIpInfo { IpAddress = remoteIpAddress, ProxyIpAddress = null };
        }
        private bool IsBlackListed(string ipAddress)
        {
            // blacklistIpAddresses.Add("192.168.1.1");
            return blacklistIpAddresses.Contains(ipAddress);
        }
    }
}