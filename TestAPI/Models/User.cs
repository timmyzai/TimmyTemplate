using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AwesomeProject;

namespace TestAPI.Models;

public class User : FullyAuditedEntity<Guid>
{
    [Key]
    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public new Guid Id { get; set; }
    public string UserName { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string CountryName { get; set; }
    public string RoleNames { get; set; }
}