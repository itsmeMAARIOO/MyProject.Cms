using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Cms.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common;

namespace MyProject.Cms.Services;
public class BookServices(IVariationContextAccessor _variationContextAccessor ,UmbracoHelper umbracoHelper, IContentService contentService, IMediaService mediaService, MediaFileManager _mediaFileManager, IShortStringHelper _shortStringHelper, IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider, MediaUrlGeneratorCollection _mediaUrlGeneratorCollection)
{
    public List<Book> DisplayBooks(string culture)
    {
        // This is how the culture is set for the context we are in
        _variationContextAccessor.VariationContext = new VariationContext(culture);

        var booksListing = umbracoHelper.Content("e86c24fb-2ca5-4d03-9e3f-ee2f132b7d73");
        var books = booksListing?.Children
            .Select(x => new Book(x))
            .ToList() ?? [];
        return books;
    }

    public List<Book> SearchBook(string bookToSearch, string culture) // is this the right data type to return?
    {
        _variationContextAccessor.VariationContext = new VariationContext(culture);

        var booksListing = umbracoHelper.Content("e86c24fb-2ca5-4d03-9e3f-ee2f132b7d73");
        bool isBookId = Guid.TryParse(bookToSearch, out var idToSearch);
        var matchedBook = booksListing?.Children
            .Where(x =>
            x.Key == idToSearch ||
            x.Name.ToLower().Contains(bookToSearch.ToLower()) ||
            x.Value("bookAuthor")?.ToString()?.ToLower() == bookToSearch.ToLower())
            .Select(x => new Book(x)
            {
                Id = x.Key,
                Name = x.Name,
                Author = x.Value<string>("bookAuthor") ?? "Unknown",
                TotalQuantity = int.TryParse(x.Value<string>("totalQuantity"), out var total) ? total : 0,
                AvailableQuantity = int.TryParse(x.Value<string>("availableQuantity"), out var available) ? available : 0,
                Cover = x.Value<MediaWithCrops>("bookCover")!.MediaUrl()
            })
            .ToList();
        if (matchedBook == null)
            return [];
        return matchedBook;
    }


    private void AddBookCover([FromForm]IFormFile? file, Guid bookId)
    {
        Guid bookCoverGuid;

        if (file == null)
            return;
        using (Stream stream = file!.OpenReadStream())
        {   
            // thanks Abdullah
            // Initialize a new image at the root of the media archive
            IMedia media = mediaService.CreateMedia(file.FileName, Constants.System.Root, Constants.Conventions.MediaTypes.Image);
            // Set the property value (Umbraco will handle the underlying magic)
            media.SetValue(_mediaFileManager, _mediaUrlGeneratorCollection, _shortStringHelper, _contentTypeBaseServiceProvider, Constants.Conventions.Media.File, file.FileName, stream);
            // get the key and pass it to be guid
            bookCoverGuid = media.Key;

            // Save to the media archive
            var result = mediaService.Save(media);
        }

        // get the book content
        IContent? newBook = contentService.GetById(bookId);

        // get the book cover to convert it to become udi
        var bookCover = umbracoHelper.Media(bookCoverGuid);

        // generate udi
        var bookCoverUdi = Udi.Create(Constants.UdiEntityType.Media, bookCover!.Key);

        // now it will be allowed to setValue
        newBook!.SetValue("bookCover", bookCoverUdi.ToString());

        // remember to save and publish
        contentService.SaveAndPublish(newBook);
    }

    public IContent AddBook(NewBookRequest newBook, string culture)
    {
        var newBookDetail = contentService.Create(newBook.Name ?? "Untitled Book", Guid.Parse("e86c24fb-2ca5-4d03-9e3f-ee2f132b7d73"), "book");
        // this is the 'Name'
        newBookDetail.SetCultureName(newBook.Name, culture);
        // this is the 'bookName' property
        newBookDetail.SetValue("bookName", newBook.Name, culture);
        newBookDetail.SetValue("bookAuthor", newBook.Author);
        newBookDetail.SetValue("totalQuantity", newBook.TotalQuantity);
        newBookDetail.SetValue("availableQuantity", newBook.AvailableQuantity);

        //newBookDetail.SetValue("bookCover", newBook.Cover); // this is not working
        contentService.SaveAndPublish(newBookDetail, culture); // save the new book first so that the id is valid
        Guid newBookId = newBookDetail.Key;
        AddBookCover(newBook.Cover, newBookId);
        return newBookDetail;
    }

    public IContent? Update(string id, NewBookRequest updateBook, string culture)
    {
        _variationContextAccessor.VariationContext = new VariationContext(culture);

        Guid idToSearch = Guid.Parse(id);
        IContent? matchedBook = contentService.GetById(idToSearch);

        if (matchedBook == null)
            return null;
        matchedBook.SetCultureName(updateBook.Name, culture);
        //matchedBook.Name = updateBook.Name;
        matchedBook.SetValue("bookName", updateBook.Name, culture);
        matchedBook.SetValue("bookAuthor", updateBook.Author);
        matchedBook.SetValue("totalQuantity", updateBook.TotalQuantity);
        matchedBook.SetValue("availableQuantity", updateBook.AvailableQuantity);
        contentService.SaveAndPublish(matchedBook, culture);
        AddBookCover(updateBook.Cover, idToSearch);

        return matchedBook;
    }

    public IContent? Delete(IPublishedContent bookToDelete)
    {
        // TAKE NOTE
        // MEDIA NORMALLY WILL JUST BE IGNORED RATHER THAN REMOVING IT 
        // IN CASE THE MEDIA IS NEEDED AGAIN

        //IContent? bookToDelete = contentService.GetById(Guid.Parse(id));
        IContent? foundDeleteBook = contentService.GetById(bookToDelete.Key);
        if (foundDeleteBook == null)
            return null;
        //var getMedia = bookToDelete.GetValue("bookCover");
        //var medias = getMedia != null ? JsonSerializer.Deserialize<List<MediaResult>>(getMedia.ToString()) : [];

        //if (medias == null)
        //    return null;
        //var mediaToDelete = mediaService.GetById(Guid.Parse(medias[0].MediaKey));
            
        //if (mediaToDelete == null)
        //    return null;
        //mediaService.Delete(mediaToDelete);
        contentService.Delete(foundDeleteBook);
        return foundDeleteBook;    
    }

    //public class MediaResult {

    //    [JsonPropertyName("key")]
    //    public string Key { get; set; } = "";

    //    [JsonPropertyName("mediaKey")]
    //    public string MediaKey { get; set; } = "";
    //}
}
