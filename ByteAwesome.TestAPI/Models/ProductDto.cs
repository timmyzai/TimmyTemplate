using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByteAwesome.TestAPI.Models
{
    public class ProductDto : FullyAuditedEntityDto
    {
        #region Properties
        
        public string Name { get; set; }
        
        public string Price { get; set; }

        public Guid Sku { get; set; }

        #endregion

    }
}