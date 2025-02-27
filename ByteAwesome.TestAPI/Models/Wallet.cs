using System.ComponentModel.DataAnnotations.Schema;

namespace ByteAwesome.TestAPI.Models;

public class Wallet : Entity<Guid>
{
    [ForeignKey("UserId")]
    public Guid UserId { get; set; }
    public decimal WalletAmount { get; set; }
}