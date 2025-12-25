using Application.Interfaces;
using Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shared.DTOs;

namespace Documents.Test.Controllers.Bookmarks;

public abstract class BookmarkControllerBase
{
    protected readonly IBookmarkService _bookmarkService;
    protected readonly BookmarkController _bookmarkController;

    protected BookmarkControllerBase()
    {
        _bookmarkService = Substitute.For<IBookmarkService>();
        _bookmarkController = new BookmarkController(_bookmarkService);
        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User")
        }));

        _bookmarkController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}
