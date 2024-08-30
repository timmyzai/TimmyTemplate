using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByteAwesome.TestAPI.Models
{
    public class ProductCreate : FullyAuditedEntityDto
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$")]
        public string Price { get; set; }
        
        [Required(ErrorMessage = "BrandId is required")]
        public int BrandId { get; set; }
        [Required(ErrorMessage = "SupplierId is required")]
        public int SupplierId { get; set; }
        [Required(ErrorMessage = "ProductTypeId is required")]
        public int ProductTypeId { get; set; }
        
        //sku will be unique for each product 
        // supplier-brands-ProductCode
        [NotMapped]
        public Guid Sku => CustomGuid;


        private Guid CustomGuid
        {
            get
            {
                // Convert each int to a byte array
                byte[] typeBytes = BitConverter.GetBytes(Id);
                byte[] brandsBytes = BitConverter.GetBytes(BrandId);
                byte[] supplierBytes = BitConverter.GetBytes(SupplierId);
                byte[] productidBytes = BitConverter.GetBytes(ProductTypeId);

                // Combine the byte arrays into a single 16-byte array
                byte[] guidBytes = new byte[16];
                Buffer.BlockCopy(typeBytes, 0, guidBytes, 0, 4);
                Buffer.BlockCopy(brandsBytes, 0, guidBytes, 4, 4);
                Buffer.BlockCopy(supplierBytes, 0, guidBytes, 8, 4);
                Buffer.BlockCopy(productidBytes, 0, guidBytes, 12, 4);

                // Create and return the GUID from the byte array
                return new Guid(guidBytes);
            }
        }
    }
}

