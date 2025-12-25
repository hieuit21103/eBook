namespace Documents.Test.Services.Documents;

public class UpdateAsyncTests : DocumentServiceBase
{
    [Fact]
    public async Task UpdateAsync_ShouldUpdateDocument_WhenValidInput()
    {
        var userId = Guid.NewGuid();
        var role = "User";
        Category category = new Category { Id = Guid.NewGuid(), Name = "Science" };
        Category updateCategory = new Category { Id = Guid.NewGuid(), Name = "Math" };
        // Arrange
        var documentId = Guid.NewGuid();
        var updateAt = DateTime.UtcNow.AddDays(-1);
        var existingDocument = new Document
        {
            Id = documentId,
            UserId = userId,
            Title = "Old Title",
            Topic = "Old Topic",
            IsDeleted = false,
            UpdatedAt = updateAt,
            Categories = new List<DocumentCategory>
            {
                new DocumentCategory
                {
                    CategoryId = category.Id,
                    Category = category
                }
            }
        };

        var updateRequest = new DocumentUpdateRequest
        {
            Title = "New Title",
            Topic = "New Topic",
            CategoryIds = new List<Guid> { updateCategory.Id }
        };

        Document? capturedDocument = null;

        _documentRepository.UpdateAsync(Arg.Do<Document>(d => capturedDocument = d)).Returns(Task.CompletedTask);
        _documentRepository.GetByIdWithDetailsAsync(documentId).Returns(existingDocument);
        _categoryRepository.ExistsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Category, bool>>>()).Returns(true);

        // Act
        await _documentService.UpdateAsync(documentId, userId, role, updateRequest);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument.Id.Should().Be(documentId);
        capturedDocument.Title.Should().Be(updateRequest.Title);
        capturedDocument.Topic.Should().Be(updateRequest.Topic);
        capturedDocument.UpdatedAt.Should().BeAfter(updateAt);

        await _documentCategoryRepository.Received(1).DeleteByDocumentIdAsync(documentId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenDocumentDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var role = "User";
        // Arrange
        var documentId = Guid.NewGuid();

        var updateRequest = new DocumentUpdateRequest
        {
            Title = "New Title",
            Topic = "New Topic",
            CategoryIds = new List<Guid> { Guid.NewGuid() }
        };

        _documentRepository.GetByIdWithDetailsAsync(documentId).Returns((Document?)null);

        // Act
        Func<Task> act = async () => await _documentService.UpdateAsync(documentId, userId, role, updateRequest);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with id {documentId} not found.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenUserIsNotOwnerOrAdmin()
    {
        var userId = Guid.NewGuid();
        var role = "User";
        // Arrange
        var documentId = Guid.NewGuid();
        var existingDocument = new Document
        {
            Id = documentId,
            UserId = Guid.NewGuid(), // Different owner
            Title = "Old Title",
            Topic = "Old Topic",
            IsDeleted = false
        };

        var updateRequest = new DocumentUpdateRequest
        {
            Title = "New Title",
            Topic = "New Topic",
            CategoryIds = new List<Guid> { Guid.NewGuid() }
        };

        _documentRepository.GetByIdWithDetailsAsync(documentId).Returns(existingDocument);

        // Act
        Func<Task> act = async () => await _documentService.UpdateAsync(documentId, userId, role, updateRequest);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You do not have permission to update this document.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var role = "User";
        Category category = new Category { Id = Guid.NewGuid(), Name = "Science" };
        // Arrange
        var documentId = Guid.NewGuid();
        var existingDocument = new Document
        {
            Id = documentId,
            UserId = userId,
            Title = "Old Title",
            Topic = "Old Topic",
            IsDeleted = false,
            Categories = new List<DocumentCategory>
            {
                new DocumentCategory
                {
                    CategoryId = category.Id,
                    Category = category
                }
            }
        };

        var invalidCategoryId = Guid.NewGuid();
        var updateRequest = new DocumentUpdateRequest
        {
            Title = "New Title",
            Topic = "New Topic",
            CategoryIds = new List<Guid> { invalidCategoryId }
        };

        _documentRepository.GetByIdWithDetailsAsync(documentId).Returns(existingDocument);
        _categoryRepository.ExistsAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Category, bool>>>()).Returns(false);
        // Act
        Func<Task> act = async () => await _documentService.UpdateAsync(documentId, userId, role, updateRequest);
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {invalidCategoryId} not found.");
    }
}