using Microsoft.AspNetCore.Mvc;
using Domain.Filters;

namespace Documents.Test.Controllers.Bookmarks;

public class GetUserBookmarksTests : BookmarkControllerBase
{
    [Fact]
    public async Task GetUserBookmarks_ShouldReturnPagedResult_WhenCalled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var pagedResult = new PagedResult<BookmarkResponse>
        {
            Items = new List<BookmarkResponse>
            {
                new BookmarkResponse { Username = "User", DocumentTitle = "Doc 1", CreatedAt = DateTime.UtcNow },
                new BookmarkResponse { Username = "User", DocumentTitle = "Doc 2", CreatedAt = DateTime.UtcNow }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _bookmarkService.GetBookmarkedDocumentsAsync(userId, username, Arg.Any<BookmarkFilterParams>())
            .Returns(pagedResult);

        // Act
        var result = await _bookmarkController.GetUserBookmarks(userId, new BookmarkFilterParams());

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<PagedResult<BookmarkResponse>>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(pagedResult);
    }
}
