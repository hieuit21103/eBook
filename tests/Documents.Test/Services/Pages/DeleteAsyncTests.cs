using Shared;

namespace Documents.Test.Services.Pages;

public class DeleteAsyncTests : PageServiceBase
{
    [Fact]
    public async Task DeleteAsync_ShouldDeletePage_WhenValidRequest()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";
        var fileId = Guid.NewGuid();

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId,
            FileId = fileId
        };

        var document = new Document
        {
            Id = documentId,
            UserId = userId,
            IsDeleted = false
        };

        _pageRepository.GetByIdAsync(pageId).Returns(page);
        _documentRepository.GetByIdAsync(documentId).Returns(document);

        // Act
        await _pageService.DeleteAsync(pageId, userId, role);

        // Assert
        await _pageRepository.Received(1).DeleteAsync(page);
        await _notificationService.Received(1).NotifyPageDeletedAsync(documentId, pageId);
        await _publishEndpoint.Received(1).Publish(Arg.Is<PageDeletedEvent>(e => e.FileId == fileId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenPageNotFound()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";

        _pageRepository.GetByIdAsync(pageId).Returns((Page?)null);

        // Act
        Func<Task> act = async () => await _pageService.DeleteAsync(pageId, userId, role);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Page with ID {pageId} not found.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenDocumentNotFound()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId
        };

        _pageRepository.GetByIdAsync(pageId).Returns(page);
        _documentRepository.GetByIdAsync(documentId).Returns((Document?)null);

        // Act
        Func<Task> act = async () => await _pageService.DeleteAsync(pageId, userId, role);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with ID {documentId} not found.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenDocumentIsDeleted()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId
        };

        var document = new Document
        {
            Id = documentId,
            IsDeleted = true
        };

        _pageRepository.GetByIdAsync(pageId).Returns(page);
        _documentRepository.GetByIdAsync(documentId).Returns(document);

        // Act
        Func<Task> act = async () => await _pageService.DeleteAsync(pageId, userId, role);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with ID {documentId} is deleted.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenUserIsNotAuthorized()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "User";

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId
        };

        var document = new Document
        {
            Id = documentId,
            UserId = Guid.NewGuid(), // Different user
            IsDeleted = false
        };

        _pageRepository.GetByIdAsync(pageId).Returns(page);
        _documentRepository.GetByIdAsync(documentId).Returns(document);

        // Act
        Func<Task> act = async () => await _pageService.DeleteAsync(pageId, userId, role);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You do not have permission to delete this page.");
    }
}
