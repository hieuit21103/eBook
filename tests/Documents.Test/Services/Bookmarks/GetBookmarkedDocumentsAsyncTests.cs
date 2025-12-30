using Domain.Filters;
using Shared.DTOs;

namespace Documents.Test.Services.Bookmarks;

public class GetBookmarkedDocumentsAsyncTests : BookmarkServiceBase
{
    [Fact]
    public async Task GetBookmarkedDocumentsAsync_ShouldReturnBookmarks_WhenExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var filterParams = new BookmarkFilterParams { PageNumber = 1, PageSize = 10 };

        var bookmarks = new PagedResult<Bookmark>
        {
            Items = new List<Bookmark> {
                new Bookmark {
                    Id = Guid.NewGuid(),
                    Page = new Page {
                        Document = new Document { Title = "Doc A", Topic = "Topic A" }
                    },
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _bookmarkRepository.GetPagedAsync(Arg.Is<BookmarkFilterParams>(x =>
                x.UserId == userId &&
                x.PageNumber == filterParams.PageNumber &&
                x.PageSize == filterParams.PageSize
            ))
            .Returns(bookmarks);

        // Act
        var result = await _bookmarkService.GetBookmarkedDocumentsAsync(userId, username, filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().DocumentTitle.Should().Be("Doc A");
    }
}
