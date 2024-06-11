using Microsoft.AspNetCore.Http;
using UAParser;

namespace ByteAwesome
{
    public class DeviceInfoMiddleware
    {
        private readonly RequestDelegate _next;

        public DeviceInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var deviceInfo = ParseUserAgent(userAgent);
            context.Items["DeviceInfo"] = deviceInfo;
            await _next(context);
        }

        private DeviceInfo ParseUserAgent(string userAgent)
        {
            ClientInfo client = Parser.GetDefault().Parse(userAgent);
            return new DeviceInfo
            {
                DeviceType = client.Device.Family,
                DeviceBrand = client.Device.Brand,
                DeviceModel = client.Device.Model,
                Os = $"{client.OS.Family} {client.OS.Major}.{client.OS.Minor}",
                Browser = $"{client.UA.Family} {client.UA.Major}.{client.UA.Minor}"
            };
        }
    }

    public class DeviceInfo
    {
        public string DeviceType { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string Os { get; set; }
        public string Browser { get; set; }
    }
}