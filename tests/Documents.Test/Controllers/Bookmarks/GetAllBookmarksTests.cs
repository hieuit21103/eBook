using Microsoft.AspNetCore.Mvc;
using Domain.Filters;

namespace Documents.Test.Controllers.Bookmarks;

public class GetAllBookmarksTests : BookmarkControllerBase
{
    [Fact]
    public async Task GetAllBookmarks_ShouldReturnPagedResult_WhenCalled()
    {
        // Arrange
        var pagedResult = new PagedResult<BookmarkResponse>
        {
            Items = new List<BookmarkResponse>
            {
                new BookmarkResponse { Username = "User1", DocumentTitle = "Doc 1", CreatedAt = DateTime.UtcNow },
                new BookmarkResponse { Username = "User2", DocumentTitle = "Doc 2", CreatedAt = DateTime.UtcNow }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _bookmarkService.GetAllBookmarksAsync(Arg.Any<BookmarkFilterParams>())
            .Returns(pagedResult);

        // Act
        var result = await _bookmarkController.GetAllBookmarks(null, null, null, null, null, null, 1, 10, "CreatedAt", true);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as PagedResult<BookmarkResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Should().BeEquivalentTo(pagedResult);
    }
}
