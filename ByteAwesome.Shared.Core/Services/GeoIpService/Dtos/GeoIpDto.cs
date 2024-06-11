namespace ByteAwesome.Services
{
    public class LocationInfo : ClientIpInfo
    {
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    public class ClientIpInfo
    {
        public string IpAddress { get; set; }
        public string ProxyIpAddress { get; set; }
    }
}