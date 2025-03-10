using System.ComponentModel.DataAnnotations;

using AwesomeProject;

namespace TestAPI.Dtos.Wallet;

public class WithdrawFromWalletDto : EntityDto<Guid>
{
    [Required]
    public decimal Amount { get; set; }
}