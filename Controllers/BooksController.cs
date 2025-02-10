using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyProject.Cms.Models;
using MyProject.Cms.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;

namespace MyProject.Cms.Controllers;

[Route("api/v1")]
[ApiController]
public class BooksController(BookServices bookServices, UmbracoHelper umbracoHelper) : UmbracoApiController
{
    [HttpGet]
    [Route("books")]
    public IActionResult DisplayAllBooks([FromQuery]string culture)
    {
        var books = bookServices.DisplayBooks(culture);
        return Ok(books);
    }

    [HttpGet]   
    [Route("books/{searchTerm}")]
    public IActionResult SearchBook(string searchTerm, [FromQuery] string? culture)
    {
        culture = culture ?? "en-US";
        var searchedBook = bookServices.SearchBook(searchTerm, culture);
        if (searchedBook.Count == 0)
            return NotFound();
        return Ok(searchedBook);
    }

    [HttpPost]
    [Route("books")]
    public IActionResult AddNewBook([FromForm]NewBookRequest newBook, [FromQuery]string? culture)
    {
        culture = culture ?? "en-US";
        var addedNewBook = bookServices.AddBook(newBook!, culture);
        return Ok(addedNewBook);
    }

    [HttpPut]
    [Route("{id}")]
    public IActionResult UpdateBook(string id, [FromForm]NewBookRequest updateBook, [FromQuery] string? culture)
    {
        culture = culture ?? "en-US";
        var editedBook = bookServices.Update(id, updateBook, culture);
        if (editedBook == null)
            return NotFound();
        return Ok(editedBook);
    }

    [HttpDelete]
    [Route("books")]
    public IActionResult DeleteBook(string id)
    {
        if (id.IsNullOrEmpty())
            return NotFound();
        var bookContent = umbracoHelper.Content(id);

        // in case of needing to delete the media (usually not needed cuz the storage is more than enuf)
        //var bookCoverMedia = bookContent.Value<MediaWithCrops>("bookCover");
        //bookCoverMedia.Key;

        if (bookContent == null)
            return NotFound();
        var deletedBook = bookServices.Delete(bookContent);
        if (deletedBook == null)
            return NotFound();
        return Ok(deletedBook);
    }
}
    