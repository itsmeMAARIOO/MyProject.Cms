using System.ComponentModel.DataAnnotations;

namespace MyProject.Cms.Models;

public class NewMemberRequest
{
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string PhoneNumber { get; set; }
}
