using FileStorage.Protos;

namespace Documents.Test.Services.Pages;

public class GetPreviewByDocumentIdAndPageNumberTests : PageServiceBase
{
    [Fact]
    public async Task GetPreviewByDocumentIdAndPageNumber_ShouldReturnPreview_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageNumber = 1;
        var fileId = Guid.NewGuid();
        var page = new Page
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            PageNumber = pageNumber,
            FileId = fileId
        };

        _pageRepository.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber)
            .Returns(page);

        var grpcResponse = new UrlResponse
        {
            FileType = FileType.Pdf,
            Url = "http://example.com/preview",
        };

        _grpcClient.GetPresignedUrl(Arg.Any<FileRequest>())
            .Returns(grpcResponse);

        var response = new PagePreviewResponse
        {
            Id = page.Id,
            DocumentId = page.DocumentId,
            PageNumber = page.PageNumber,
            FileType = grpcResponse.FileType,
            Url = grpcResponse.Url
        };

        // Act
        var result = await _pageService.GetPreviewByDocumentIdAndPageNumberAsync(documentId, pageNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(response, result);
    }

    [Fact]
    public async Task GetPreviewByDocumentIdAndPageNumber_ShouldThrowException_WhenPageDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageNumber = 1;

        _pageRepository.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber)
            .Returns((Page?)null);

        // Act
        Func<Task> act = async () => await _pageService.GetPreviewByDocumentIdAndPageNumberAsync(documentId, pageNumber);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
                    .WithMessage($"Page number {pageNumber} not found for document ID {documentId}.");
    }
}