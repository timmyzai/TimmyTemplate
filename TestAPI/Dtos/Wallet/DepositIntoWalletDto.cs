using System.ComponentModel.DataAnnotations;

namespace TestAPI.Dtos.Wallet;

public class DepositIntoWalletDto
{
    [Required]
    public decimal Amount { get; set; }
}