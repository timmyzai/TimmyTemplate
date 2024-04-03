using ByteAwesome.Services.Entities;
using ByteAwesome.Services.TestAPI.Models.Dtos;

namespace ByteAwesome.Services.TestAPI.Models
{
    public class Product : FullyAuditedEntity<Guid>
    {
       public string Name { get; set; }
        public ProductType SKU { get; set; }
        public float Price { get; set; }
    }
}