using FileStorage.Domain.Enums;

namespace FileStorage.Application.DTOs;

public class FileDownloadResponse
{
    public Stream FileStream { get; set; } = Stream.Null;
    public FileType FileType { get; set; }
    public string FileName { get; set; } = string.Empty;
}
