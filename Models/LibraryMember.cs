using System.ComponentModel.DataAnnotations;

namespace MyProject.Cms.Models;

public class LibraryMember
{
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string PhoneNumber { get; set; }
    [Required]
    public DateTime RegisterDate { get; set; }
    public List<Book>? BorrowedBook { get; set; }
}
