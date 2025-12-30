using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Documents;

public class UpdateTests : DocumentControllerBase
{
    [Fact]
    public async Task Update_ShouldReturnUpdatedDocument_WhenValidRequest()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var request = new DocumentUpdateRequest
        {
            Title = "Updated Title",
            Topic = "Updated Topic",
            CategoryIds = new List<Guid> { Guid.NewGuid() }
        };

        var updatedDocument = new DocumentResponse
        {
            Id = documentId,
            Title = request.Title,
            Topic = request.Topic,
            UpdatedAt = DateTime.UtcNow
        };

        _documentService.UpdateAsync(documentId, Arg.Any<Guid>(), Arg.Any<string>(),
            Arg.Is<DocumentUpdateRequest>(r => r.Title == request.Title && r.Topic == request.Topic))
            .Returns(updatedDocument);

        // Act
        var result = await _documentController.Update(documentId, request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<DocumentResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(updatedDocument);
    }
}
