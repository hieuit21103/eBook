namespace Application.DTOs.Page;

public class PageDownloadResponse
{
    public Stream Content { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}