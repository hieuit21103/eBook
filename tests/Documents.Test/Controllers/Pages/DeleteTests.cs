using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Documents.Test.Controllers.Pages;

public class DeleteTests : PageControllerBase
{
    [Fact]
    public async Task Delete_ShouldReturnSuccessMessage_WhenPageExists()
    {
        // Arrange
        var pageId = Guid.NewGuid();

        _pageService.DeleteAsync(pageId, Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _pageController.Delete(pageId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Page deleted successfully");
    }
}