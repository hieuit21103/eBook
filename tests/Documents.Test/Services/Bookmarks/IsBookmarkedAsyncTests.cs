using System.Linq.Expressions;

namespace Documents.Test.Services.Bookmarks;

public class IsBookmarkedAsyncTests : BookmarkServiceBase
{
    [Fact]
    public async Task IsBookmarkedAsync_ShouldReturnTrue_WhenBookmarked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        _bookmarkRepository.ExistsAsync(Arg.Any<Expression<Func<Bookmark, bool>>>()).Returns(true);

        // Act
        var result = await _bookmarkService.IsBookmarkedAsync(pageId, userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsBookmarkedAsync_ShouldReturnFalse_WhenNotBookmarked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        _bookmarkRepository.ExistsAsync(Arg.Any<Expression<Func<Bookmark, bool>>>()).Returns(false);

        // Act
        var result = await _bookmarkService.IsBookmarkedAsync(pageId, userId);

        // Assert
        result.Should().BeFalse();
    }
}
