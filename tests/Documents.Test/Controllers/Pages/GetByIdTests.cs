using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;
using Application.DTOs.Page;

namespace Documents.Test.Controllers.Pages;

public class GetByIdTests : PageControllerBase
{
    [Fact]
    public async Task GetById_ShouldReturnPage_WhenExists()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var pageResponse = new PageResponse
        {
            Id = pageId,
            DocumentId = Guid.NewGuid(),
            PageNumber = 1,
            DocumentTitle = "Test Document"
        };

        _pageService.GetByIdAsync(pageId).Returns(pageResponse);

        // Act
        var result = await _pageController.GetById(pageId);

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