using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MyProject.Cms.Models;

public class Book
{
    //private IFormFile? cover;

    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Author { get; set; }
    [Required]
    public int TotalQuantity { get; set; }
    [Required]
    public int AvailableQuantity { get; set; }
    public string Cover { get; set; }


    public Book(IPublishedContent content)
    {
        Id = content.Key;
        Name = content.Name;
        Author = content.Value<string>("bookAuthor") ?? "Unknown Author";
        TotalQuantity = content.Value<int>("totalQuantity");
        AvailableQuantity = content.Value<int>("availableQuantity");
        Cover = content.Value<MediaWithCrops>("bookCover")!.MediaUrl();
    }
}
