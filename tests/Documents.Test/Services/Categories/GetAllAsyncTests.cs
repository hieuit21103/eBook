using Domain.Filters;
using Shared.DTOs;

namespace Documents.Test.Services.Categories;

public class GetAllAsyncTests : CategoryServiceBase
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnCategories_WhenExist()
    {
        // Arrange
        var filterParams = new CategoryFilterParams { PageNumber = 1, PageSize = 10 };

        var categories = new PagedResult<Category>
        {
            Items = new List<Category> {
                new Category { Id = Guid.NewGuid(), Name = "Science" },
                new Category { Id = Guid.NewGuid(), Name = "Math" }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _categoryRepository.GetPagedAsync(filterParams).Returns(categories);

        // Act
        var result = await _categoryService.GetAllAsync(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.First().Name.Should().Be("Science");
    }
}
