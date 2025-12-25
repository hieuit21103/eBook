using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Pages;

public class UpdateTests : PageControllerBase
{
    [Fact]
    public async Task Update_ShouldReturnUpdatedPage_WhenValidRequest()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var request = new PageUpdateRequest
        {
            PageNumber = 2,
            Content = Substitute.For<IFormFile>()
        };

        var updatedPage = new PageResponse
        {
            Id = pageId,
            DocumentId = Guid.NewGuid(),
            PageNumber = request.PageNumber,
            DocumentTitle = "Test Document"
        };

        _pageService.UpdateAsync(pageId, Arg.Any<Guid>(), Arg.Any<string>(),
            Arg.Is<PageUpdateRequest>(r => r.PageNumber == request.PageNumber && r.Content == request.Content))
            .Returns(updatedPage);

        // Act
        var result = await _pageController.Update(pageId, request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<PageResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(updatedPage);
    }
}