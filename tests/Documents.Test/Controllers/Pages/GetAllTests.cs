using Domain.Filters;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Application.DTOs.Page;

namespace Documents.Test.Controllers.Pages;

public class GetAllTests : PageControllerBase
{
    [Fact]
    public async Task GetAll_ShouldReturnPagedResult_WhenCalled()
    {
        // Arrange
        var pagedResult = new PagedResult<PageResponse>
        {
            Items = new List<PageResponse>
            {
                new PageResponse { Id = Guid.NewGuid(), DocumentId = Guid.NewGuid(), PageNumber = 1 },
                new PageResponse { Id = Guid.NewGuid(), DocumentId = Guid.NewGuid(), PageNumber = 2 }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _pageService.GetAllAsync(Arg.Any<PageFilterParams>())
            .Returns(pagedResult);

        // Act
        var result = await _pageController.GetAll(new PageFilterParams());

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<PagedResult<PageResponse>>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(pagedResult);
    }
}