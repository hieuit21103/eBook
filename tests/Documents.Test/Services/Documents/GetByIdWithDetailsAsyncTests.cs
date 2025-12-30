namespace Documents.Test.Services.Documents;

public class GetByIdWithDetailsAsyncTests : DocumentServiceBase
{
    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnDocumentWithDetails_WhenDocumentExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            Title = "Test Document",
            Topic = "Test Topic",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TotalPages = 10,
            IsDeleted = false,
            Pages = new List<Page>(),
            Categories = new List<DocumentCategory>()
        };

        var response = new DocumentResponse
        {
            Id = document.Id,
            Title = document.Title,
            Topic = document.Topic,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            TotalPages = document.TotalPages,
            Categories = new List<string>(),
        };

        _documentRepository.GetByIdWithDetailsAsync(documentId).Returns(document);

        // Act
        var result = await _documentService.GetByIdWithDetailsAsync(documentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldThrowException_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        _documentRepository.GetByIdWithDetailsAsync(documentId).Returns((Document?)null);

        // Act
        Func<Task> act = async () => await _documentService.GetByIdWithDetailsAsync(documentId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with id {documentId} not found.");
    }
}