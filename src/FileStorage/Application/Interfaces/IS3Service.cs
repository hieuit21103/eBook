namespace FileStorage.Application.Interfaces;

public interface IS3Service
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string s3Key);
    Task<bool> DeleteFileAsync(string s3Key);
    Task<bool> FileExistsAsync(string s3Key);
    Task<string> GetPresignedUrlAsync(string s3Key);
}
