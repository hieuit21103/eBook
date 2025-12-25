using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;

namespace FileStorage.Test.Services.FileStorage;

public class DownloadFileAsyncTests : FileStorageServiceBase
{
    [Fact]
    public async Task DownloadFileAsync_ShouldReturnFileStream_WhenFileExists()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var metadata = new FileMetadata
        {
            Id = fileId,
            FileName = "test.pdf",
            FilePath = "2023/12/25/test.pdf",
            FileType = FileType.pdf
        };

        var expectedStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

        _fileMetadataRepository.GetByIdAsync(fileId).Returns(metadata);
        _s3Service.DownloadFileAsync(metadata.FilePath).Returns(expectedStream);

        // Act
        var result = await _fileStorageService.DownloadFileAsync(fileId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStream, result.FileStream);
        Assert.Equal(metadata.FileName, result.FileName);
        Assert.Equal(metadata.FileType, result.FileType);
    }

    [Fact]
    public async Task DownloadFileAsync_ShouldThrowFileNotFoundException_WhenMetadataNotFound()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _fileMetadataRepository.GetByIdAsync(fileId).Returns((FileMetadata?)null);

        // Act
        Func<Task> act = async () => await _fileStorageService.DownloadFileAsync(fileId);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("File not found");
    }
}
