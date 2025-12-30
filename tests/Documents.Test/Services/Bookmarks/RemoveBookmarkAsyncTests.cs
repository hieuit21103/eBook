using System.Linq.Expressions;

namespace Documents.Test.Services.Bookmarks;

public class RemoveBookmarkAsyncTests : BookmarkServiceBase
{
    [Fact]
    public async Task RemoveBookmarkAsync_ShouldRemoveBookmark_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        var bookmark = new Bookmark { Id = Guid.NewGuid(), PageId = pageId, UserId = userId };

        _bookmarkRepository.FindAsync(Arg.Any<Expression<Func<Bookmark, bool>>>()).Returns(new List<Bookmark> { bookmark });

        // Act
        await _bookmarkService.RemoveBookmarkAsync(pageId, userId);

        // Assert
        await _bookmarkRepository.Received(1).DeleteAsync(bookmark);
    }

    [Fact]
    public async Task RemoveBookmarkAsync_ShouldThrowException_WhenBookmarkDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        _bookmarkRepository.FindAsync(Arg.Any<Expression<Func<Bookmark, bool>>>()).Returns(new List<Bookmark>());

        // Act
        Func<Task> act = async () => await _bookmarkService.RemoveBookmarkAsync(pageId, userId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookmark not found.");
    }
}
