using Domain.Entities;
using Application.DTOs.Document;

namespace Documents.Test.Services.Documents;

public class GetByIdAsyncTests() : DocumentServiceBase
{

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDocument_WhenDocumentExists()
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
            Categories = new List<string>()
        };

        _documentRepository.GetByIdAsync(documentId).Returns(document);

        // Act
        var result = await _documentService.GetByIdAsync(documentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowException_WhenDocumentDoesNotExist()
    {
        var documentId = Guid.NewGuid();
        _documentRepository.GetByIdAsync(documentId).Returns((Document?)null);

        // Act
        Func<Task> act = async () => await _documentService.GetByIdAsync(documentId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with id {documentId} not found.");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowException_WhenDocumentIsDeletedd()
    {
        var documentId = Guid.NewGuid();
        var deletedDocument = new Document
        {
            Id = documentId,
            IsDeleted = true
        };

        _documentRepository.GetByIdAsync(documentId).Returns(deletedDocument);

        // Act
        Func<Task> act = async () => await _documentService.GetByIdAsync(documentId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with id {documentId} not found.");
    }
}