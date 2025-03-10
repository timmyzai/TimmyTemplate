using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace TestAPI.Dtos.Wallet;

public class CreateWalletDto
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    [Required]
    public string CountryName {get; set;}
}