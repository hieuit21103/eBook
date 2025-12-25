using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Application.DTOs.Page;

namespace Documents.Test.Controllers.Pages;

public class GetPreviewByDocumentIdAndPageNumberTests : PageControllerBase
{
    [Fact]
    public async Task GetPreviewByDocumentIdAndPageNumber_ShouldReturnPreview_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageNumber = 1;
        var previewResponse = new PagePreviewResponse
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            PageNumber = pageNumber,
            Url = "http://example.com/preview"
        };

        _pageService.GetPreviewByDocumentIdAndPageNumberAsync(documentId, pageNumber)
            .Returns(previewResponse);

        // Act
        var result = await _pageController.GetPreviewByDocumentIdAndPageNumber(documentId, pageNumber);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<PagePreviewResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(previewResponse);
    }
}