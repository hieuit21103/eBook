using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Categories;

public class ExistsTests : CategoryControllerBase
{
    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryService.ExistsAsync(categoryId).Returns(true);

        // Act
        var result = await _categoryController.Exists(categoryId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<bool>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Exists_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryService.ExistsAsync(categoryId).Returns(false);

        // Act
        var result = await _categoryController.Exists(categoryId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<bool>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeFalse();
    }
}
