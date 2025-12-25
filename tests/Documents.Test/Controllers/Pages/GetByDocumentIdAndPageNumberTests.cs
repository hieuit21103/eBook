using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Application.DTOs.Page;

namespace Documents.Test.Controllers.Pages;

public class GetByDocumentIdAndPageNumberTests : PageControllerBase
{
    [Fact]
    public async Task GetByDocumentIdAndPageNumber_ShouldReturnPage_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var pageNumber = 1;
        var pageResponse = new PageResponse
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            PageNumber = pageNumber,
            DocumentTitle = "Test Document"
        };

        _pageService.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber)
            .Returns(pageResponse);

        // Act
        var result = await _pageController.GetByDocumentIdAndPageNumber(documentId, pageNumber);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<PageResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(pageResponse);
    }
}