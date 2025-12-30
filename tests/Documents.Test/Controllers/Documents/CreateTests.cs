using Microsoft.AspNetCore.Mvc;

namespace Documents.Test.Controllers.Documents;

public class CreateTests : DocumentControllerBase
{
    [Fact]
    public async Task Create_ShouldReturnCreatedDocument_WhenValidRequest()
    {
        // Arrange
        var request = new DocumentCreateRequest
        {
            Title = "New Document",
            Topic = "New Topic",
            CategoryIds = new List<Guid> { Guid.NewGuid() }
        };

        var createdDocument = new DocumentResponse
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Topic = request.Topic,
            CreatedAt = DateTime.UtcNow
        };

        _documentService.CreateAsync(Arg.Any<Guid>(), Arg.Is<DocumentCreateRequest>(r =>
            r.Title == request.Title && r.Topic == request.Topic))
            .Returns(createdDocument);

        // Act
        var result = await _documentController.Create(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<DocumentResponse>;
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(createdDocument);
    }
}
