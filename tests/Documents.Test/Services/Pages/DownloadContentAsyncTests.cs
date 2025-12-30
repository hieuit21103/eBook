using FileStorage.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Documents.Test.Services.Pages;

public class DownloadContentAsyncTests : PageServiceBase
{
    [Fact]
    public async Task DownloadContentAsync_ShouldReturnPageDownloadResponse_WhenPageExists()
    {
        var pageId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        _pageRepository.GetByIdAsync(pageId).Returns(new Page
        {
            Id = pageId,
            FileId = fileId
        });

        var msgMetadata = new FileDownloadResponse
        {
            Metadata = new FileDownloadMetadata
            {
                FileName = "page1.pdf",
                FileType = FileType.Pdf
            }
        };

        var byteData = new byte[] { 0x01, 0x02, 0x03 };
        var msgChunk = new FileDownloadResponse
        {
            ChunkData = Google.Protobuf.ByteString.CopyFrom(byteData)
        };

        var responseStreamList = new List<FileDownloadResponse> { msgMetadata, msgChunk };

        using var iterator = responseStreamList.GetEnumerator();
        var mockStreamReader = Substitute.For<IAsyncStreamReader<FileDownloadResponse>>();

        mockStreamReader.MoveNext(Arg.Any<CancellationToken>())
            .Returns(async _ =>
            {
                await Task.Yield();
                return iterator.MoveNext();
            });

        mockStreamReader.Current.Returns(_ => iterator.Current);

        var fakeCall = new AsyncServerStreamingCall<FileDownloadResponse>(
            responseStream: mockStreamReader,
            responseHeadersAsync: Task.FromResult(new Metadata()),
            getStatusFunc: () => Status.DefaultSuccess,
            getTrailersFunc: () => new Metadata(),
            disposeAction: () => { }
        );

        _grpcClient.DownloadFile(
                Arg.Any<FileRequest>(),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>())
            .Returns(fakeCall);

        // Act
        var result = await _pageService.DownloadContentAsync(pageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("page1.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
        using var ms = new MemoryStream();
        await result.Content.CopyToAsync(ms);
        Assert.Equal(byteData, ms.ToArray());
    }

    [Fact]
    public async Task DownloadContentAsync_ShouldThrowException_WhenPageDoesNotExist()
    {
        var pageId = Guid.NewGuid();

        _pageRepository.GetByIdAsync(pageId).Returns((Page?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _pageService.DownloadContentAsync(pageId);
        });
    }

    [Fact]
    public async Task DownloadContentAsync_ShouldThrowException_WhenFileIdIsNull()
    {
        var pageId = Guid.NewGuid();

        _pageRepository.GetByIdAsync(pageId).Returns(new Page
        {
            Id = pageId,
            FileId = null
        });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _pageService.DownloadContentAsync(pageId);
        });
    }
}