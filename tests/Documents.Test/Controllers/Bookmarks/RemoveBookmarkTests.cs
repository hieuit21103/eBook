using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Bookmarks;

public class RemoveBookmarkTests : BookmarkControllerBase
{
    [Fact]
    public async Task RemoveBookmark_ShouldReturnSuccessMessage_WhenBookmarkExists()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bookmarkService.RemoveBookmarkAsync(pageId, userId)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _bookmarkController.RemoveBookmark(pageId, userId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Bookmark removed successfully");
    }
}
