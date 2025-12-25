using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Bookmarks;

public class CheckBookmarkTests : BookmarkControllerBase
{
    [Fact]
    public async Task CheckBookmark_ShouldReturnTrue_WhenBookmarked()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bookmarkService.IsBookmarkedAsync(pageId, userId).Returns(true);

        // Act
        var result = await _bookmarkController.CheckBookmark(pageId, userId);

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
    public async Task CheckBookmark_ShouldReturnFalse_WhenNotBookmarked()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bookmarkService.IsBookmarkedAsync(pageId, userId).Returns(false);

        // Act
        var result = await _bookmarkController.CheckBookmark(pageId, userId);

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
