namespace Documents.Test.Services.Pages;

public class GetByIdAsyncTests : PageServiceBase
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnPage_WhenExists()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId,
            Document = new Document { Id = documentId, Title = "Test Document" },
            PageNumber = 1,
            FileId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _pageRepository.GetByIdWithDetailsAsync(pageId).Returns(page);

        // Act
        var result = await _pageService.GetByIdAsync(pageId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(pageId);
        result.DocumentTitle.Should().Be("Test Document");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowException_WhenPageDoesNotExist()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        _pageRepository.GetByIdWithDetailsAsync(pageId).Returns((Page?)null);

        // Act
        Func<Task> act = async () => await _pageService.GetByIdAsync(pageId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Page with ID {pageId} not found.");
    }
}
