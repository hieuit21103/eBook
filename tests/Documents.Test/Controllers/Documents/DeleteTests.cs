using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Documents.Test.Controllers.Documents;

public class DeleteTests : DocumentControllerBase
{
    [Fact]
    public async Task Delete_ShouldReturnSuccessMessage_WhenDocumentExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _documentService.DeleteAsync(documentId, Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _documentController.Delete(documentId);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<string>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().Be("Document deleted successfully.");
    }
}
