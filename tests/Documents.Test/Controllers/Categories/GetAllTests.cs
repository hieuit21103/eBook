using Microsoft.AspNetCore.Mvc;
using Domain.Filters;

namespace Documents.Test.Controllers.Categories;

public class GetAllTests : CategoryControllerBase
{
    [Fact]
    public async Task GetAll_ShouldReturnPagedResult_WhenCalled()
    {
        // Arrange
        var pagedResult = new PagedResult<CategoryResponse>
        {
            Items = new List<CategoryResponse>
            {
                new CategoryResponse { Id = Guid.NewGuid(), Name = "Science" },
                new CategoryResponse { Id = Guid.NewGuid(), Name = "Math" }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _categoryService.GetAllAsync(Arg.Any<CategoryFilterParams>())
            .Returns(pagedResult);

        // Act
        var result = await _categoryController.GetAll(null, null, null, null, 1, 10, "CreatedAt", false);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<PagedResult<CategoryResponse>>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(pagedResult);
    }
}
