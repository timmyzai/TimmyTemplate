using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ByteAwesome.TestAPI.Models;

namespace ByteAwesome.TestAPI.entity
{

    public class Product: FullyAuditedEntity<Guid>
    {
        public string Name { get; set; }
        
        public string Price { get; set; }
        
        public int BrandId { get; set; }
        public int SupplierId { get; set; }
        public int ProductTypeId { get; set; }
        
        [ForeignKey("BrandId")]
        public Brand Brand { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }
        
        [ForeignKey("TypeId")]
        public ProductType ProductType { get; set; }
        
        public Guid Sku { get; set; }


    }
}