using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Categories;

public class UpdateTests : CategoryControllerBase
{
    [Fact]
    public async Task Update_ShouldReturnUpdatedCategory_WhenValidRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new CategoryUpdateRequest { Name = "Updated Science" };

        var updatedCategory = new CategoryResponse
        {
            Id = categoryId,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        _categoryService.UpdateAsync(categoryId, Arg.Is<CategoryUpdateRequest>(r => r.Name == request.Name))
            .Returns(updatedCategory);

        // Act
        var result = await _categoryController.Update(categoryId, request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<CategoryResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(updatedCategory);
    }
}
