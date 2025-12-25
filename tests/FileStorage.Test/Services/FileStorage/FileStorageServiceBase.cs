using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FileStorage.Test.Services.FileStorage;

public abstract class FileStorageServiceBase
{
    protected readonly IS3Service _s3Service;
    protected readonly IFileMetadataRepository _fileMetadataRepository;
    protected readonly ILogger<FileStorageService> _logger;
    protected readonly FileStorageService _fileStorageService;

    protected FileStorageServiceBase()
    {
        _s3Service = Substitute.For<IS3Service>();
        _fileMetadataRepository = Substitute.For<IFileMetadataRepository>();
        _logger = Substitute.For<ILogger<FileStorageService>>();
        _fileStorageService = new FileStorageService(_s3Service, _fileMetadataRepository, _logger);
    }
}