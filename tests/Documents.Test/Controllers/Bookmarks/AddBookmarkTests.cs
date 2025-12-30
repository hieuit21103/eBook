using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Bookmarks;

public class AddBookmarkTests : BookmarkControllerBase
{
    [Fact]
    public async Task AddBookmark_ShouldReturnBookmarkResponse_WhenValidRequest()
    {
        // Arrange
        var username = "testuser";
        var request = new BookmarkCreateRequest
        {
            PageId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        var bookmarkResponse = new BookmarkResponse
        {
            Username = "User",
            DocumentTitle = "Test Document",
            DocumentTopic = "Test Topic",
            CreatedAt = DateTime.UtcNow
        };

        _bookmarkService.AddBookmarkAsync(username, Arg.Is<BookmarkCreateRequest>(r =>
            r.PageId == request.PageId && r.UserId == request.UserId))
            .Returns(bookmarkResponse);

        // Act
        var result = await _bookmarkController.AddBookmark(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<BookmarkResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Bookmark added successfully");
        apiResponse.Data.Should().BeEquivalentTo(bookmarkResponse);
    }
}
