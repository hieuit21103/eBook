using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Documents.Test.Controllers.Documents;

public class GetByIdTests : DocumentControllerBase
{
    [Fact]
    public async Task GetById_ShouldReturnDocument_WhenExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentResponse = new DocumentResponse
        {
            Id = documentId,
            Title = "Test Document",
            Topic = "Test Topic"
        };

        _documentService.GetByIdAsync(documentId).Returns(documentResponse);

        // Act
        var result = await _documentController.GetById(documentId);

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
