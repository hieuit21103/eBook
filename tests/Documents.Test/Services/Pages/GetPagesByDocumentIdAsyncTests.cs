using Domain.Filters;

namespace Documents.Test.Services.Pages;

public class GetPagesByDocumentIdAsyncTests : PageServiceBase
{
    [Fact]
    public async Task GetPagesByDocumentIdAsync_ShouldReturnPages_WhenExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var filterParams = new PageFilterParams { PageNumber = 1, PageSize = 10 };

        var pages = new List<Page> {
            new Page { Id = Guid.NewGuid(), DocumentId = documentId, PageNumber = 1, Document = new Document { Title = "Doc A" } },
            new Page { Id = Guid.NewGuid(), DocumentId = documentId, PageNumber = 2, Document = new Document { Title = "Doc A" } }
        };

        _pageRepository.GetPagesByDocumentIdAsync(documentId).Returns(pages);

        // Act
        var result = await _pageService.GetPagesByDocumentIdAsync(documentId, filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetPagesByDocumentIdAsync_ShouldReturnEmptyList_WhenNoPagesExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var filterParams = new PageFilterParams { PageNumber = 1, PageSize = 10 };

        _pageRepository.GetPagesByDocumentIdAsync(documentId).Returns(new List<Page>());

        // Act
        var result = await _pageService.GetPagesByDocumentIdAsync(documentId, filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
