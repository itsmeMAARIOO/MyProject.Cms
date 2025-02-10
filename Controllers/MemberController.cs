using Microsoft.AspNetCore.Mvc;
using MyProject.Cms.Models;
using MyProject.Cms.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace MyProject.Cms.Controllers;

[Route("api/v1")]
[ApiController]
public class MemberController(LibraryMemberServices memberServices) : UmbracoApiController
{
    [HttpPost]
    [Route("members")]
    // rmb for the [FromForm] as it is the proper way to get the data from the user
    public IActionResult RegisterMember([FromForm]NewMemberRequest newMember) 
    {
        var newLibraryMember = memberServices.Register(newMember);
        return Ok(newLibraryMember);
    }

    [HttpPut]
    [Route("{memberId}/borrow")]
    public IActionResult BorrowBook(string memberId, string bookId)
    {
        var borrowedBook = memberServices.Borrow(memberId, bookId);
        return Ok(borrowedBook);
    }
}
