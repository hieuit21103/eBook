using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;

namespace FileStorage.Test.Services.FileStorage;

public class GetPresignedUrlAsyncTests : FileStorageServiceBase
{
    [Fact]
    public async Task GetPresignedUrlAsync_ShouldReturnUrl_WhenFileExists()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var metadata = new FileMetadata
        {
            Id = fileId,
            FilePath = "path/to/file",
            FileType = FileType.pdf
        };
        var expectedUrl = "https://s3.example.com/file";

        _fileMetadataRepository.GetByIdAsync(fileId).Returns(metadata);
        _s3Service.GetPresignedUrlAsync(metadata.FilePath).Returns(expectedUrl);

        // Act
        var result = await _fileStorageService.GetPresignedUrlAsync(fileId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUrl, result.Url);
        Assert.Equal(metadata.FileType, result.FileType);
    }

    [Fact]
    public async Task GetPresignedUrlAsync_ShouldThrowFileNotFoundException_WhenMetadataNotFound()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _fileMetadataRepository.GetByIdAsync(fileId).Returns((FileMetadata?)null);

        // Act
        Func<Task> act = async () => await _fileStorageService.GetPresignedUrlAsync(fileId);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("File not found");
    }

    [Fact]
    public async Task GetPresignedUrlAsync_ShouldThrowException_WhenUrlGenerationFails()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var metadata = new FileMetadata
        {
            Id = fileId,
            FilePath = "path/to/file"
        };

        _fileMetadataRepository.GetByIdAsync(fileId).Returns(metadata);
        _s3Service.GetPresignedUrlAsync(metadata.FilePath).Returns((string?)null);

        // Act
        Func<Task> act = async () => await _fileStorageService.GetPresignedUrlAsync(fileId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Failed to generate presigned URL");
    }
}
