using FileStorage.Application.DTOs;
using FileStorage.Application.Interfaces;
using FileStorage.Domain.Entities;
using FileStorage.Domain.Enums;
using FileStorage.Domain.Interfaces;

namespace FileStorage.Application.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IS3Service _s3Service;
    private readonly IFileMetadataRepository _fileMetadataRepository;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IS3Service s3Service,
        IFileMetadataRepository fileMetadataRepository,
        ILogger<FileStorageService> logger)
    {
        _s3Service = s3Service;
        _fileMetadataRepository = fileMetadataRepository;
        _logger = logger;
    }

    public async Task<FileUploadResponse> UploadFileAsync(Stream stream, string fileName, string contentType, string filePath)
    {
        try
        {
            if (stream.Length == 0)
                throw new ArgumentException("File is empty");

            var key = $"{filePath}/{fileName}";
            var s3Key = await _s3Service.UploadFileAsync(stream, key, contentType);

            if (string.IsNullOrEmpty(s3Key))
                throw new Exception("Failed to upload file to S3");

            var fileMetadata = new FileMetadata
            {
                Id = Guid.NewGuid(),
                FileType = DetermineFileType(fileName),
                FileName = fileName,
                FilePath = s3Key,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var savedMetadata = await _fileMetadataRepository.CreateAsync(fileMetadata);

            return new FileUploadResponse
            {
                Id = savedMetadata.Id,
                FileType = savedMetadata.FileType,
                FileName = savedMetadata.FileName,
                FilePath = savedMetadata.FilePath,
                CreatedAt = savedMetadata.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            throw;
        }
    }

    public async Task<FileDownloadResponse> DownloadFileAsync(Guid fileId)
    {
        var metadata = await _fileMetadataRepository.GetByIdAsync(fileId);
        if (metadata == null)
            throw new FileNotFoundException("File not found");

        var stream = await _s3Service.DownloadFileAsync(metadata.FilePath);

        return new FileDownloadResponse
        {
            FileStream = stream,
            FileType = metadata.FileType,
            FileName = metadata.FileName
        };
    }

    public async Task<FileMetadataResponse?> GetFileMetadataAsync(Guid fileId)
    {
        var metadata = await _fileMetadataRepository.GetByIdAsync(fileId);
        if (metadata == null)
            return null;

        return MapToResponse(metadata);
    }

    public async Task<bool> DeleteFileAsync(Guid fileId)
    {
        var metadata = await _fileMetadataRepository.GetByIdAsync(fileId);
        if (metadata == null)
            return false;

        // Delete from S3
        await _s3Service.DeleteFileAsync(metadata.FilePath);
        
        // Delete from database
        await _fileMetadataRepository.DeleteAsync(fileId);

        return true;
    }

    public async Task<PresignedUrlResponse> GetPresignedUrlAsync(Guid fileId)
    {
        var metadata = await _fileMetadataRepository.GetByIdAsync(fileId);
        if (metadata == null)
            throw new FileNotFoundException("File not found");

        var url = await _s3Service.GetPresignedUrlAsync(metadata.FilePath);
        if (string.IsNullOrEmpty(url))
            throw new Exception("Failed to generate presigned URL");
        
        return new PresignedUrlResponse
        {
            FileType = metadata.FileType,
            Url = url
        };
    }

    private static FileMetadataResponse MapToResponse(FileMetadata metadata)
    {
        return new FileMetadataResponse
        {
            Id = metadata.Id,
            FileType = metadata.FileType,
            FileName = metadata.FileName,
            FilePath = metadata.FilePath,
            CreatedAt = metadata.CreatedAt,
            UpdatedAt = metadata.UpdatedAt
        };
    }

    private static FileType DetermineFileType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => FileType.pdf,
            ".txt" => FileType.text,
            ".xls" or ".xlsx" => FileType.excel,
            _ => FileType.text
        };
    }
}
