using FileStorage.Protos;

namespace Application.DTOs.Page;

public class PagePreviewResponse
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int PageNumber { get; set; }
    public FileType FileType { get; set; }
    public string Url { get; set; } = string.Empty;
}