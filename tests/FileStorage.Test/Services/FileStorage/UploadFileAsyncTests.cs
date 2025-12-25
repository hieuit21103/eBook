using System.IO;

namespace FileStorage.Test.Services.FileStorage;

public class UploadFileAsyncTests : FileStorageServiceBase
{
    [Fact]
    public async Task UploadFileAsync_ShouldReturnFileUploadResponse_WhenValidFile()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var fileName = "test.pdf";
        var contentType = "application/pdf";
        var filePath = "documents";
        var s3Key = "documents/test.pdf";
        var fileId = Guid.NewGuid();

        _s3Service.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), contentType).Returns(s3Key);

        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            FileType = FileType.pdf,
            FileName = fileName,
            FilePath = s3Key,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _fileMetadataRepository.CreateAsync(Arg.Any<FileMetadata>()).Returns(fileMetadata);

        // Act
        var result = await _fileStorageService.UploadFileAsync(stream, fileName, contentType, filePath);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(fileId);
        result.FileName.Should().Be(fileName);
        result.FilePath.Should().Be(s3Key);
        result.FileType.Should().Be(FileType.pdf);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldThrowException_WhenFileIsEmpty()
    {
        // Arrange
        var stream = new MemoryStream();
        var fileName = "empty.pdf";
        var contentType = "application/pdf";
        var filePath = "documents";

        // Act
        Func<Task> act = async () => await _fileStorageService.UploadFileAsync(stream, fileName, contentType, filePath);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("File is empty");
    }
}