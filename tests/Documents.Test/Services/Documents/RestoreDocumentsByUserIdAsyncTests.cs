namespace Documents.Test.Services.Documents;

public class RestoreDocumentsByUserIdAsyncTests : DocumentServiceBase
{
    [Fact]
    public async Task RestoreDocumentsByUserIdAsync_ShouldRestoreDeletedDocuments_WhenDocumentsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var documents = new List<Document>
        {
            new Document { Id = Guid.NewGuid(), UserId = userId, IsDeleted = true },
            new Document { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false } // Should not be updated
        };

        _documentRepository.GetByUserIdAsync(userId).Returns(documents);

        // Act
        await _documentService.RestoreDocumentsByUserIdAsync(userId);

        // Assert
        documents[0].IsDeleted.Should().BeFalse();
        await _documentRepository.Received(1).UpdateAsync(documents[0]);

        await _documentRepository.DidNotReceive().UpdateAsync(documents[1]);
    }

    [Fact]
    public async Task RestoreDocumentsByUserIdAsync_ShouldDoNothing_WhenNoDocumentsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _documentRepository.GetByUserIdAsync(userId).Returns(new List<Document>());

        // Act
        await _documentService.RestoreDocumentsByUserIdAsync(userId);

        // Assert
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>());
    }
}
