using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Documents.Test.Controllers.Documents;

namespace Documents.Test.Controllers.Documents;

public class GetByIdWithDetailsTests : DocumentControllerBase
{
    [Fact]
    public async Task GetByIdWithDetails_ShouldReturnDocumentWithDetails_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentResponse = new DocumentResponse
        {
            Id = documentId,
            Title = "Test Document",
            Topic = "Test Topic",
            Categories = new List<string> { "Science", "Math" }
        };

        _documentService.GetByIdWithDetailsAsync(documentId).Returns(documentResponse);

        // Act
        var result = await _documentController.GetByIdWithDetails(documentId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<DocumentResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(documentResponse);
    }
}
