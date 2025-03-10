using TestAPI.Helper.CustomDataAttributes;

namespace TestAPI.Dtos.Wallet;

public class CreateWalletDto
{
    [RequiredValidGuid]
    public Guid UserId { get; set; }
}