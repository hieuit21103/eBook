namespace Documents.Test.Services.Documents;

public class DeleteDocumentsByUserIdAsyncTests : DocumentServiceBase
{
    [Fact]
    public async Task DeleteDocumentsByUserIdAsync_ShouldMarkDocumentsAsDeleted_WhenDocumentsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var documents = new List<Document>
        {
            new Document { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false },
            new Document { Id = Guid.NewGuid(), UserId = userId, IsDeleted = false }
        };

        _documentRepository.GetByUserIdAsync(userId).Returns(documents);

        // Act
        await _documentService.DeleteDocumentsByUserIdAsync(userId);

        // Assert
        foreach (var document in documents)
        {
            document.IsDeleted.Should().BeTrue();
            await _documentRepository.Received(1).UpdateAsync(document);
        }
    }

    [Fact]
    public async Task DeleteDocumentsByUserIdAsync_ShouldDoNothing_WhenNoDocumentsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _documentRepository.GetByUserIdAsync(userId).Returns(new List<Document>());

        // Act
        await _documentService.DeleteDocumentsByUserIdAsync(userId);

        // Assert
        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>());
    }
}
