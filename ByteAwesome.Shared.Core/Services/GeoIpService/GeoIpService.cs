using MaxMind.Db;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Http;

namespace ByteAwesome.Services
{
    public interface IGeoIPService
    {
        LocationInfo GetLocationFromIp(ClientIpInfo clientIpInfo);
    }
    public class GeoIPService : IGeoIPService
    {
        private readonly DatabaseReader _reader;
        private readonly IHttpContextAccessor httpContextAccessor;

        public GeoIPService(string databasePath, IHttpContextAccessor httpContextAccessor)
        {
            _reader = new DatabaseReader(databasePath, FileAccessMode.Memory);
            this.httpContextAccessor = httpContextAccessor;
        }
        public LocationInfo GetLocationFromIp(ClientIpInfo clientIpInfo)
        {
            try
            {
                if (GeneralHelper.IsDevelopmentEnvironment() || IsGrpcRequest())
                {
                    return LocalLocationInfo(clientIpInfo);
                }
                return GetLocationFromDatabase(clientIpInfo);
            }
            catch (Exception ex)
            {
                ActionResultHandler.HandleException(ex);
                return UnknownLocationInfo(clientIpInfo);
            }
        }
        private LocationInfo GetLocationFromDatabase(ClientIpInfo clientIpInfo)
        {
            var city = _reader.City(clientIpInfo.IpAddress);
            if (city is not null)
            {
                return new LocationInfo
                {
                    IpAddress = clientIpInfo.IpAddress,
                    ProxyIpAddress = clientIpInfo.ProxyIpAddress,
                    Country = city.Country.Name,
                    Region = city.MostSpecificSubdivision.Name,
                    City = city.City.Name,
                    PostalCode = city.Postal.Code,
                    Latitude = city.Location.Latitude ?? 0,
                    Longitude = city.Location.Longitude ?? 0
                };
            }
            return UnknownLocationInfo(clientIpInfo);
        }
        private LocationInfo LocalLocationInfo(ClientIpInfo clientIpInfo)
        {
            return new LocationInfo
            {
                IpAddress = clientIpInfo.IpAddress,
                ProxyIpAddress = clientIpInfo.ProxyIpAddress,
                Country = "Local",
                Region = "Local",
                City = "Local",
                PostalCode = "Local",
                Latitude = 0,
                Longitude = 0
            };
        }
        private LocationInfo UnknownLocationInfo(ClientIpInfo clientIpInfo)
        {
            return new LocationInfo
            {
                IpAddress = clientIpInfo.IpAddress,
                ProxyIpAddress = clientIpInfo.ProxyIpAddress,
                Country = "Unknown",
                Region = "Unknown",
                City = "Unknown",
                PostalCode = "Unknown",
                Latitude = 0,
                Longitude = 0
            };
        }
        private bool IsGrpcRequest()
        {
            var context = httpContextAccessor.HttpContext;
            var result = context?.Request.ContentType == "application/grpc";
            return result;
        }
    }
}