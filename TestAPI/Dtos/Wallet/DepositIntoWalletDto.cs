using System.ComponentModel.DataAnnotations;

using AwesomeProject;

namespace TestAPI.Dtos.Wallet;

public class DepositIntoWalletDto : EntityDto<Guid>
{
    [Required]
    public decimal Amount { get; set; }
}