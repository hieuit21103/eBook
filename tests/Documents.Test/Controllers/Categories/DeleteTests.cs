using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Categories;

public class DeleteTests : CategoryControllerBase
{
    [Fact]
    public async Task Delete_ShouldReturnSuccessMessage_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        _categoryService.DeleteAsync(categoryId)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _categoryController.Delete(categoryId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<string>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be("Category deleted successfully.");
    }
}
