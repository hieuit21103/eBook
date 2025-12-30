using FileStorage.Protos;
using Grpc.Core;
using Shared;

namespace Documents.Test.Services.Pages;

public class UpdateAsyncTests : PageServiceBase
{
    [Fact]
    public async Task UpdateAsync_ShouldUpdatePage_WhenValidRequest()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";
        var oldFileId = Guid.NewGuid();

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId,
            PageNumber = 1,
            FileId = oldFileId
        };

        var document = new Document
        {
            Id = documentId,
            UserId = userId,
            IsDeleted = false
        };

        _pageRepository.GetByIdAsync(pageId).Returns(page);
        _documentRepository.GetByIdAsync(documentId).Returns(document);

        var request = new PageUpdateRequest
        {
            PageNumber = 2
        };

        // Act
        var result = await _pageService.UpdateAsync(pageId, userId, role, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PageNumber);
        await _pageRepository.Received(1).UpdateAsync(page);
        await _notificationService.Received(1).NotifyPageUpdatedAsync(documentId, pageId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenPageNotFound()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";
        var request = new PageUpdateRequest();

        _pageRepository.GetByIdAsync(pageId).Returns((Page?)null);

        // Act
        Func<Task> act = async () => await _pageService.UpdateAsync(pageId, userId, role, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Page with ID {pageId} not found.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenDocumentNotFound()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";
        var request = new PageUpdateRequest();

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId
        };

        _pageRepository.GetByIdAsync(pageId).Returns(page);
        _documentRepository.GetByIdAsync(documentId).Returns((Document?)null);

        // Act
        Func<Task> act = async () => await _pageService.UpdateAsync(pageId, userId, role, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Document with ID {documentId} not found.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenDocumentIsDeleted()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";
        var request = new PageUpdateRequest();

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
        Func<Task> act = async () => await _pageService.UpdateAsync(pageId, userId, role, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Document with ID {documentId} is deleted.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenUserIsNotAuthorized()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "User";
        var request = new PageUpdateRequest();

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
        Func<Task> act = async () => await _pageService.UpdateAsync(pageId, userId, role, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You do not have permission to update this page.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldShiftPages_WhenPageNumberChangedAndConflictExists()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";
        var request = new PageUpdateRequest { PageNumber = 2 };

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId,
            PageNumber = 1
        };

        var document = new Document
        {
            Id = documentId,
            UserId = userId
        };

        var existingPage = new Page { Id = Guid.NewGuid(), PageNumber = 2, DocumentId = documentId };
        var page3 = new Page { Id = Guid.NewGuid(), PageNumber = 3, DocumentId = documentId };

        _pageRepository.GetByIdAsync(pageId).Returns(page);
        _documentRepository.GetByIdAsync(documentId).Returns(document);

        _pageRepository.GetByDocumentIdAndPageNumberAsync(documentId, 2)
            .Returns(existingPage);

        _pageRepository.GetPagesGreaterThanOrEqualToAsync(documentId, 2)
            .Returns(new List<Page> { existingPage, page3 });

        // Act
        await _pageService.UpdateAsync(pageId, userId, role, request);

        // Assert
        await _pageRepository.Received(1).UpdateAsync(existingPage);
        await _pageRepository.Received(1).UpdateAsync(page3);
        Assert.Equal(3, existingPage.PageNumber);
        Assert.Equal(4, page3.PageNumber);
        Assert.Equal(2, page.PageNumber);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUploadNewFile_WhenContentIsProvided()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";
        var oldFileId = Guid.NewGuid();
        var newFileId = Guid.NewGuid();

        var file = Substitute.For<IFormFile>();
        file.OpenReadStream().Returns(new MemoryStream(new byte[] { 0x01 }));

        var request = new PageUpdateRequest
        {
            PageNumber = 1,
            Content = file
        };

        var page = new Page
        {
            Id = pageId,
            DocumentId = documentId,
            PageNumber = 1,
            FileId = oldFileId
        };

        var document = new Document
        {
            Id = documentId,
            UserId = userId
        };

        _pageRepository.GetByIdAsync(pageId).Returns(page);
        _documentRepository.GetByIdAsync(documentId).Returns(document);

        // Mock gRPC upload
        var grpcResponse = new FileUploadResponse { Id = newFileId.ToString() };
        var requestStream = Substitute.For<IClientStreamWriter<UploadFileRequest>>();
        requestStream.WriteAsync(Arg.Any<UploadFileRequest>()).Returns(Task.CompletedTask);
        requestStream.CompleteAsync().Returns(Task.CompletedTask);

        var call = new AsyncClientStreamingCall<UploadFileRequest, FileUploadResponse>(
            requestStream,
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );

        _grpcClient.UploadFile(Arg.Any<Metadata>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(call);

        // Act
        await _pageService.UpdateAsync(pageId, userId, role, request);

        // Assert
        Assert.Equal(newFileId, page.FileId);
        await _publishEndpoint.Received(1).Publish(Arg.Is<PageUpdatedEvent>(e => e.OldFileId == oldFileId));
    }
}
