using FileStorage.Protos;
using Grpc.Core;

namespace Documents.Test.Services.Pages;

public class CreateAsyncTests : PageServiceBase
{
    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedPage_WhenValidRequest()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";

        // Mock document
        var document = new Document
        {
            Id = documentId,
            Title = "Test Document",
            UserId = userId
        };
        _documentRepository.GetByIdAsync(documentId)
            .Returns(document);

        // Mock file
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("page1.pdf");
        file.Length.Returns(1024);
        file.OpenReadStream().Returns(new MemoryStream(new byte[] { 0x01, 0x02, 0x03 }));

        // Mock request
        var request = new PageCreateRequest
        {
            PageNumber = 1,
            DocumentId = documentId,
            Content = file
        };

        // Mock gRPC upload response
        var expectedFileId = Guid.NewGuid();
        var grpcResponse = new FileUploadResponse
        {
            Id = expectedFileId.ToString()
        };

        // Setup the gRPC client mock
        var requestStream = Substitute.For<IClientStreamWriter<UploadFileRequest>>();
        requestStream.WriteAsync(Arg.Any<UploadFileRequest>())
            .Returns(Task.CompletedTask);
        requestStream.CompleteAsync()
            .Returns(Task.CompletedTask);
        var call = new AsyncClientStreamingCall<UploadFileRequest, FileUploadResponse>(
            requestStream,
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );

        _grpcClient.UploadFile(
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(call);


        // Mock repository
        var createdPage = new PageResponse
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            PageNumber = request.PageNumber,
            DocumentTitle = "Test Document",
            FileId = Guid.Parse(grpcResponse.Id)
        };

        _pageRepository.AddAsync(Arg.Any<Page>())
            .Returns(new Page
            {
                Id = createdPage.Id,
                DocumentId = documentId,
                Document = document,
                PageNumber = request.PageNumber,
                FileId = Guid.Parse(grpcResponse.Id)
            });

        // Act
        var result = await _pageService.CreateAsync(userId, role, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(createdPage, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenDocumentNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";

        var request = new PageCreateRequest
        {
            PageNumber = 1,
            DocumentId = documentId,
            Content = Substitute.For<IFormFile>()
        };

        _documentRepository.GetByIdAsync(documentId)
            .Returns((Document?)null);

        // Act 
        Func<Task> act = async () => await _pageService.CreateAsync(userId, role, request);
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with ID {documentId} not found.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenDocumentIsDeleted()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";

        var request = new PageCreateRequest
        {
            PageNumber = 1,
            DocumentId = documentId,
            Content = Substitute.For<IFormFile>()
        };

        var document = new Document
        {
            Id = documentId,
            IsDeleted = true
        };

        _documentRepository.GetByIdAsync(documentId)
            .Returns(document);

        // Act 
        Func<Task> act = async () => await _pageService.CreateAsync(userId, role, request);
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Document with ID {documentId} is deleted.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenUserIsNotAuthorized()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "User";

        // Mock document
        var document = new Document
        {
            Id = documentId,
            Title = "Test Document",
            UserId = Guid.NewGuid()
        };
        _documentRepository.GetByIdAsync(documentId)
            .Returns(document);

        // Mock file
        var file = Substitute.For<IFormFile>();

        // Mock request
        var request = new PageCreateRequest
        {
            PageNumber = 1,
            DocumentId = documentId,
            Content = file
        };

        // Act 
        Func<Task> act = async () => await _pageService.CreateAsync(userId, role, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You do not have permission to create a page for this document.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenFileUploadFails()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";

        // Mock document
        var document = new Document
        {
            Id = documentId,
            Title = "Test Document",
            UserId = userId
        };
        _documentRepository.GetByIdAsync(documentId)
            .Returns(document);

        // Mock file
        var file = Substitute.For<IFormFile>();
        file.OpenReadStream().Returns(new MemoryStream(new byte[] { 0x01 }));

        // Mock request
        var request = new PageCreateRequest
        {
            PageNumber = 1,
            DocumentId = documentId,
            Content = file
        };

        // Mock gRPC upload response
        var expectedFileId = Guid.NewGuid();
        var grpcResponse = new FileUploadResponse
        {
            Id = expectedFileId.ToString()
        };

        // Setup the gRPC client mock
        var requestStream = Substitute.For<IClientStreamWriter<UploadFileRequest>>();
        requestStream.WriteAsync(Arg.Any<UploadFileRequest>())
            .Returns(Task.CompletedTask);
        requestStream.CompleteAsync()
            .Returns(x => throw new Exception("File upload failed."));
        var call = new AsyncClientStreamingCall<UploadFileRequest, FileUploadResponse>(
            requestStream,
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { }
        );

        _grpcClient.UploadFile(
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(call);


        // Mock repository
        var createdPage = new PageResponse
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            PageNumber = request.PageNumber,
            DocumentTitle = "Test Document",
            FileId = Guid.Parse(grpcResponse.Id)
        };

        _pageRepository.AddAsync(Arg.Any<Page>())
            .Returns(new Page
            {
                Id = createdPage.Id,
                DocumentId = documentId,
                Document = document,
                PageNumber = request.PageNumber,
                FileId = Guid.Parse(grpcResponse.Id)
            });

        // Act
        Func<Task> act = async () => await _pageService.CreateAsync(userId, role, request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("File upload failed.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenRpcCallFails()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = "Admin";

        // Mock document
        var document = new Document
        {
            Id = documentId,
            Title = "Test Document",
            UserId = userId
        };
        _documentRepository.GetByIdAsync(documentId)
            .Returns(document);

        // Mock gRPC exception
        var rpcException = new RpcException(new Status(StatusCode.Unavailable, "File upload failed due to RPC error."));
        _grpcClient.UploadFile(
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(x => throw rpcException);

        // Mock file
        var file = Substitute.For<IFormFile>();

        // Mock request
        var request = new PageCreateRequest
        {
            PageNumber = 1,
            DocumentId = documentId,
            Content = file
        };

        // Act
        Func<Task> act = async () => await _pageService.CreateAsync(userId, role, request);

        // Assert
        await act.Should().ThrowAsync<RpcException>()
            .Where(ex => ex.StatusCode == StatusCode.Unavailable && ex.Status.Detail == "File upload failed due to RPC error.");
    }
}