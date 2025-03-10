using System.ComponentModel.DataAnnotations.Schema;
using AwesomeProject;

namespace TestAPI.Models;

public class Wallet : Entity<Guid>
{
    [ForeignKey("UserId")]
    public Guid UserId { get; set; }
    public decimal WalletAmount { get; set; }
}