using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ByteAwesome.Services.EntitiesDto;
using ByteAwesome.Services.TestAPI.Helper;
namespace ByteAwesome.Services.TestAPI.Models.Dtos
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public ProductType SKU { get; set; }
        [Required]
        public float Price { get; set; }
    }
    public class ProductDto : FullyAuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public ProductType SKU { get; set; }
        public float Price { get; set; }
    }
    public enum ProductType
    {
        [Description("Sneakers")]
        Sneakers = 0,
        [Description("Phone")]
        Phone = 1,
        [Description("Car")]
        Car = 2
    }
}
