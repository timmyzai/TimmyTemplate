using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.TestAPI.entity
{

    public class Supplier : FullyAuditedEntity
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

    }
}