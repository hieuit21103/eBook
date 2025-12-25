using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Categories;

public class GetByIdTests : CategoryControllerBase
{
    [Fact]
    public async Task GetById_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var categoryResponse = new CategoryResponse
        {
            Id = categoryId,
            Name = "Science",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _categoryService.GetByIdAsync(categoryId).Returns(categoryResponse);

        // Act
        var result = await _categoryController.GetById(categoryId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<CategoryResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(categoryResponse);
    }
}
