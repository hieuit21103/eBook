using Application.Interfaces;
using Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shared.DTOs;

namespace Documents.Test.Controllers.Pages;

public abstract class PageControllerBase
{
    protected readonly IPageService _pageService;
    protected readonly PageController _pageController;

    protected PageControllerBase()
    {
        _pageService = Substitute.For<IPageService>();
        _pageController = new PageController(_pageService);
        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User")
        }));

        _pageController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}