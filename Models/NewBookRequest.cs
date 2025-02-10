using System.ComponentModel.DataAnnotations;

namespace MyProject.Cms.Models;

public class NewBookRequest
{
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Author { get; set; }
    [Required]
    public int TotalQuantity { get; set; }
    [Required]
    public int AvailableQuantity { get; set; }
    public IFormFile? Cover { get; set; }
}
