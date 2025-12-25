namespace Documents.Test.Services.Pages;

public class GetByDocumentIdAndPageNumberAsyncTests : PageServiceBase
{
    [Fact]
    public async Task GetByDocumentIdAndPageNumberAsync_ShouldReturnPage_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageNumber = 1;

        var page = new Page
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            PageNumber = pageNumber,
            Document = new Document { Id = documentId, Title = "Test Document" }
        };

        _pageRepository.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber).Returns(page);

        // Act
        var result = await _pageService.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(pageNumber);
        result.DocumentId.Should().Be(documentId);
    }

    [Fact]
    public async Task GetByDocumentIdAndPageNumberAsync_ShouldThrowException_WhenPageDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageNumber = 1;

        _pageRepository.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber).Returns((Page?)null);

        // Act
        Func<Task> act = async () => await _pageService.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Page number {pageNumber} not found for document ID {documentId}.");
    }
}
