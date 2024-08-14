

namespace ByteAwesome.UserAPI.Models.Dtos
{
    public class UserLoginSessionInfoDto : EntityDto<Guid>
    {
        public Guid UsersId { get; set; }
        public string DeviceType { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string OS { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string ProxyIpAddress { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastLoginTime { get; set; }
    }
}