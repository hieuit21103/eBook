using Domain.Filters;
using Shared.DTOs;

namespace Documents.Test.Services.Bookmarks;

public class GetAllBookmarksAsyncTests : BookmarkServiceBase
{
    [Fact]
    public async Task GetAllBookmarksAsync_ShouldReturnAllBookmarks_WhenExist()
    {
        // Arrange
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

        _bookmarkRepository.GetPagedAsync(filterParams).Returns(bookmarks);

        // Act
        var result = await _bookmarkService.GetAllBookmarksAsync(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().DocumentTitle.Should().Be("Doc A");
    }
}
