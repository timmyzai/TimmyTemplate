using System.ComponentModel.DataAnnotations;
using ByteAwesome.TestAPI.Helper.CustomDataAttributes;

namespace ByteAwesome.TestAPI.Dtos.Wallet;

public class CreateWalletDto
{
    [RequiredValidGuid]
    public Guid UserId { get; set; }
}