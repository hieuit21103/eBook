using Domain.Interfaces;
using Application.Services;
using Domain.Entities;
using NSubstitute;

namespace Documents.Test.Services.Categories;

public abstract class CategoryServiceBase
{
    protected readonly ICategoryRepository _categoryRepository;
    protected readonly CategoryService _categoryService;

    protected CategoryServiceBase()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _categoryService = new CategoryService(_categoryRepository);
    }
}
