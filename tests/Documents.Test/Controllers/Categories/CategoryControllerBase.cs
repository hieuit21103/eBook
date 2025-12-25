using Application.Interfaces;
using Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shared.DTOs;

namespace Documents.Test.Controllers.Categories;

public abstract class CategoryControllerBase
{
    protected readonly ICategoryService _categoryService;
    protected readonly CategoryController _categoryController;

    protected CategoryControllerBase()
    {
        _categoryService = Substitute.For<ICategoryService>();
        _categoryController = new CategoryController(_categoryService);
        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin") // CategoryController requires Admin role
        }));

        _categoryController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}
