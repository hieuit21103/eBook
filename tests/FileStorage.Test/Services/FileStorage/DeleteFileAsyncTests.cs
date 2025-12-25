using FileStorage.Domain.Entities;

namespace FileStorage.Test.Services.FileStorage;

public class DeleteFileAsyncTests : FileStorageServiceBase
{
    [Fact]
    public async Task DeleteFileAsync_ShouldReturnTrue_WhenFileExists()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var metadata = new FileMetadata
        {
            Id = fileId,
            FilePath = "path/to/file"
        };

        _fileMetadataRepository.GetByIdAsync(fileId).Returns(metadata);

        // Act
        var result = await _fileStorageService.DeleteFileAsync(fileId);

        // Assert
        Assert.True(result);
        await _s3Service.Received(1).DeleteFileAsync(metadata.FilePath);
        await _fileMetadataRepository.Received(1).DeleteAsync(fileId);
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldReturnFalse_WhenMetadataNotFound()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _fileMetadataRepository.GetByIdAsync(fileId).Returns((FileMetadata?)null);

        // Act
        var result = await _fileStorageService.DeleteFileAsync(fileId);

        // Assert
        Assert.False(result);
        await _s3Service.DidNotReceive().DeleteFileAsync(Arg.Any<string>());
        await _fileMetadataRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }
}
