using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.TestAPI.Dtos.Wallet;

public class WithdrawFromWalletDto : EntityDto<Guid>
{
    [Required]
    public decimal Amount { get; set; }
}