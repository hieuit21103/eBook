using Domain.Filters;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Documents.Test.Controllers.Documents;

namespace Documents.Test.Controllers.Documents;

public class GetAllTests : DocumentControllerBase
{
    [Fact]
    public async Task GetAll_ShouldReturnPagedResult_WhenCalled()
    {
        // Arrange
        var pagedResult = new PagedResult<DocumentResponse>
        {
            Items = new List<DocumentResponse>
            {
                new DocumentResponse { Id = Guid.NewGuid(), Title = "Doc 1" },
                new DocumentResponse { Id = Guid.NewGuid(), Title = "Doc 2" }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _documentService.GetAllAsync(Arg.Any<DocumentFilterParams>())
            .Returns(pagedResult);

        // Act
        var result = await _documentController.GetAll(null, null, null, null, null, null, null, 1, 10, "CreatedAt", true);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<PagedResult<DocumentResponse>>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(pagedResult);
    }
}
