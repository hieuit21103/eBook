using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Application.DTOs.Page;

namespace Documents.Test.Controllers.Pages;

public class CreateTests : PageControllerBase
{
    [Fact]
    public async Task Create_ShouldReturnCreatedPage_WhenValidRequest()
    {
        // Arrange
        var request = new PageCreateRequest
        {
            DocumentId = Guid.NewGuid(),
            PageNumber = 1,
            Content = Substitute.For<IFormFile>()
        };

        var createdPage = new PageResponse
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            PageNumber = request.PageNumber,
            DocumentTitle = "Test Document"
        };

        _pageService.CreateAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Is<PageCreateRequest>(r =>
            r.DocumentId == request.DocumentId && r.PageNumber == request.PageNumber))
            .Returns(createdPage);

        // Act
        var result = await _pageController.Create(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<PageResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(createdPage);
    }
}