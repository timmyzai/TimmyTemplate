using System.ComponentModel.DataAnnotations;

namespace ByteAwesome.TestAPI.Dtos.Wallet;

public class CreateWalletDto
{
    [Required]
    public Guid UserId { get; set; }
}