using AwesomeProject;

namespace TestAPI.Dtos.Wallet;

public class WalletDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public decimal WalletAmount { get; set; }
}