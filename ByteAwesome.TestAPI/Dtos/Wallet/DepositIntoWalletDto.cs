using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.TestAPI.Dtos.Wallet;

public class DepositIntoWalletDto : EntityDto<Guid>
{
    [Required]
    public decimal Amount { get; set; }
}