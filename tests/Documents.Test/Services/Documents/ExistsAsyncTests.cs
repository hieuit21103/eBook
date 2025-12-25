using System.Linq.Expressions;

namespace Documents.Test.Services.Documents;

public class ExistsAsyncTests : DocumentServiceBase
{
    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenDocumentExistsAndIsNotDeleted()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _documentRepository.ExistsAsync(Arg.Any<Expression<Func<Document, bool>>>()).Returns(true);

        // Act
        var result = await _documentService.ExistsAsync(documentId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _documentRepository.ExistsAsync(Arg.Any<Expression<Func<Document, bool>>>()).Returns(false);

        // Act
        var result = await _documentService.ExistsAsync(documentId);

        // Assert
        result.Should().BeFalse();
    }
}
