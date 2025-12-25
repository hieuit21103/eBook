namespace Documents.Test.Services.Documents;

public class DeleteAsyncTests : DocumentServiceBase
{
    [Fact]
    public async Task DeleteAsync_ShouldMarkDocumentAsDeleted_WhenDocumentExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = "User";
        var documentId = Guid.NewGuid();
        var savedDocument = new Document
        {
            Id = documentId,
            UserId = userId,
            IsDeleted = false
        };

        _documentRepository.GetByIdAsync(documentId).Returns(savedDocument);

        // Act
        await _documentService.DeleteAsync(documentId, userId, role);

        // Assert
        savedDocument.IsDeleted.Should().BeTrue();
        await _documentRepository.Received(1).UpdateAsync(savedDocument);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        var role = "User";
        var userId = Guid.NewGuid();
        _documentRepository.GetByIdAsync(documentId).Returns((Document?)null);

        // Act
        Func<Task> act = async () => await _documentService.DeleteAsync(documentId, userId, role);
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with id {documentId} not found.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenUserIsNotOwnerOrAdmin()
    {
        var documentId = Guid.NewGuid();
        var role = "User";
        var userId = Guid.NewGuid();
        var existingDocument = new Document
        {
            Id = documentId,
            UserId = Guid.NewGuid(), // Different owner
            IsDeleted = false
        };

        _documentRepository.GetByIdAsync(documentId).Returns(existingDocument);

        // Act
        Func<Task> act = async () => await _documentService.DeleteAsync(documentId, userId, role);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You do not have permission to delete this document.");
    }
}