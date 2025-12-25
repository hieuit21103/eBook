using System.Linq.Expressions;
using Domain.Filters;
using Shared.DTOs;

namespace Documents.Test.Services.Bookmarks;

public class AddBookmarkAsyncTests : BookmarkServiceBase
{
    [Fact]
    public async Task AddBookmarkAsync_ShouldAddBookmark_WhenValidInput()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId,
            Document = new Document { Id = documentId, Title = "Test Document", Topic = "Test Topic" }
        };

        var request = new BookmarkCreateRequest
        {
            PageId = pageId,
            UserId = userId
        };

        var savedBookmark = new Bookmark
        {
            Id = Guid.NewGuid(),
            PageId = pageId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _pageRepository.GetByIdWithDetailsAsync(pageId).Returns(page);
        _bookmarkRepository.FindAsync(Arg.Any<Expression<Func<Bookmark, bool>>>()).Returns(new List<Bookmark>());
        _bookmarkRepository.AddAsync(Arg.Any<Bookmark>()).Returns(savedBookmark);

        // Act
        var result = await _bookmarkService.AddBookmarkAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.DocumentTitle.Should().Be("Test Document");
        result.DocumentTopic.Should().Be("Test Topic");
        await _bookmarkRepository.Received(1).AddAsync(Arg.Any<Bookmark>());
    }

    [Fact]
    public async Task AddBookmarkAsync_ShouldThrowException_WhenPageDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        var request = new BookmarkCreateRequest
        {
            PageId = pageId,
            UserId = userId
        };

        _pageRepository.GetByIdWithDetailsAsync(pageId).Returns((Page?)null);

        // Act
        Func<Task> act = async () => await _bookmarkService.AddBookmarkAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Page with ID {pageId} not found.");
    }

    [Fact]
    public async Task AddBookmarkAsync_ShouldThrowException_WhenAlreadyBookmarked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        var request = new BookmarkCreateRequest
        {
            PageId = pageId,
            UserId = userId
        };

        var existingBookmark = new Bookmark { Id = Guid.NewGuid(), PageId = pageId, UserId = userId };

        _pageRepository.GetByIdWithDetailsAsync(pageId).Returns(new Page { Id = pageId });
        _bookmarkRepository.FindAsync(Arg.Any<Expression<Func<Bookmark, bool>>>()).Returns(new List<Bookmark> { existingBookmark });

        // Act
        Func<Task> act = async () => await _bookmarkService.AddBookmarkAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This page is already bookmarked by this user.");
    }
}
