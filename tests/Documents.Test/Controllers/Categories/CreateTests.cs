using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Categories;

public class CreateTests : CategoryControllerBase
{
    [Fact]
    public async Task Create_ShouldReturnCreatedCategory_WhenValidRequest()
    {
        // Arrange
        var request = new CategoryCreateRequest { Name = "Science" };

        var createdCategory = new CategoryResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _categoryService.CreateAsync(Arg.Is<CategoryCreateRequest>(r => r.Name == request.Name))
            .Returns(createdCategory);

        // Act
        var result = await _categoryController.Create(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<CategoryResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(createdCategory);
    }
}
