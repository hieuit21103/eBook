using FileStorage.Application.DTOs;

namespace FileStorage.Application.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResponse> UploadFileAsync(Stream stream, string fileName, string fileType, string filePath);
    Task<FileDownloadResponse> DownloadFileAsync(Guid fileId);
    Task<FileMetadataResponse?> GetFileMetadataAsync(Guid fileId);
    Task<bool> DeleteFileAsync(Guid fileId);
    Task<PresignedUrlResponse> GetPresignedUrlAsync(Guid fileId);
}
