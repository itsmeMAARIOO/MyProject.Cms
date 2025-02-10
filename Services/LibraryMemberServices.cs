using MyProject.Cms.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Core;

namespace MyProject.Cms.Services;

public class LibraryMemberServices(IContentService contentService, UmbracoHelper umbracoHelper)
{
    public IContent Register(NewMemberRequest newMember)
    {
        var newMemberDetails = contentService.Create(newMember.Name ?? "Unnamed Member", Guid.Parse("214c25eb-7b29-48a1-b1f7-93d37c81ada3"), "libraryMember");
        newMemberDetails.SetValue("memberName", newMember.Name);
        newMemberDetails.SetValue("phoneNumber", newMember.PhoneNumber);
        newMemberDetails.SetValue("email", newMember.Email);
        newMemberDetails.SetValue("registerDate", DateTime.Now);
        newMemberDetails.SetValue("borrowedBook", null);

        contentService.SaveAndPublish(newMemberDetails);

        return newMemberDetails;
    }

    public IContent? Borrow(string memberId, string bookId)
    {
        var libraryMember = contentService.GetById(Guid.Parse(memberId));
        var matchedBook = umbracoHelper.Content(Guid.Parse(bookId));
        
        // if member and searched book exist
        if (libraryMember == null || matchedBook == null)
            return null;
        var borrowedBooks = libraryMember.GetValue<string>("borrowedBook")?.Split(',').ToList() ?? [];
        var newBorrowedBook = umbracoHelper.Content(matchedBook.Key);

        // check if the book exist
        if (newBorrowedBook == null)
            return null;
        var newBorrowedBookUdi = Udi.Create(Constants.UdiEntityType.Document, newBorrowedBook.Key);

        // validation for borrowing books (repeated book / book count > 5)
        if (borrowedBooks.Contains(newBorrowedBookUdi.ToString()) || borrowedBooks.Count > 5)
            return null;
        borrowedBooks.Add(newBorrowedBookUdi.ToString());

        libraryMember.SetValue("borrowedBook", string.Join(",", borrowedBooks));

        contentService.SaveAndPublish(libraryMember);
        return libraryMember;
    }

}
