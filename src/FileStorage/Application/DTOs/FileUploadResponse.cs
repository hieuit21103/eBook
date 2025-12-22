using FileStorage.Domain.Enums;

namespace FileStorage.Application.DTOs;

public class FileUploadResponse
{
    public Guid Id { get; set; }
    public FileType FileType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
