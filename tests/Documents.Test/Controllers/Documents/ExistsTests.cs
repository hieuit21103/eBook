using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Documents;
public class ExistsTests : DocumentControllerBase
{
    [Fact]
    public async Task Exists_ShouldReturnTrue_WhenDocumentExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _documentService.ExistsAsync(documentId).Returns(true);

        // Act
        var result = await _documentController.Exists(documentId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<bool>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Exists_ShouldReturnFalse_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _documentService.ExistsAsync(documentId).Returns(false);

        // Act
        var result = await _documentController.Exists(documentId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<bool>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeFalse();
    }
}
