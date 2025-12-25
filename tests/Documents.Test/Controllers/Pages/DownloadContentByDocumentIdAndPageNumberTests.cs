using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Page;

namespace Documents.Test.Controllers.Pages;

public class DownloadContentByDocumentIdAndPageNumberTests : PageControllerBase
{
    [Fact]
    public async Task DownloadContentByDocumentIdAndPageNumber_ShouldReturnFile_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageNumber = 1;
        var pageId = Guid.NewGuid();
        var pageResponse = new PageResponse
        {
            Id = pageId,
            DocumentId = documentId,
            PageNumber = pageNumber
        };

        var fileContent = new PageDownloadResponse
        {
            Content = new MemoryStream(new byte[] { 1, 2, 3 }),
            ContentType = "application/pdf",
            FileName = "page.pdf"
        };

        _pageService.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber)
            .Returns(pageResponse);

        _pageService.DownloadContentAsync(pageId)
            .Returns(fileContent);

        // Act
        var result = await _pageController.DownloadContentByDocumentIdAndPageNumber(documentId, pageNumber);

        // Assert
        var fileResult = result as FileStreamResult;
        fileResult.Should().NotBeNull();
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("page.pdf");
    }
}