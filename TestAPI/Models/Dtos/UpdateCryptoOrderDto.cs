using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestAPI.Models.Dtos
{
    public class UpdateCryptoOrderDto

    {
        [Required] public Guid id { get; set; }

        [Required] public double UsdAmount { get; set; }
        [Required] public OrderType Type { get; set; }
    }
}