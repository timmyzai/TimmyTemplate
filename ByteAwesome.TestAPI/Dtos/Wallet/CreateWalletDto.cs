namespace ByteAwesome.TestAPI.Dtos.Wallet;

public class CreateWalletDto
{
    public Guid UserId { get; set; }
    public decimal WalletAmount { get; set; }
}