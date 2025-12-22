using FileStorage.Domain.Enums;

namespace FileStorage.Application.DTOs;

public class PresignedUrlResponse
{
    public FileType FileType { get; set; }
    public string Url { get; set; } = string.Empty;
}