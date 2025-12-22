namespace Application.DTOs.Document;

public class DocumentUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalPages { get; set; }
    public List<Guid> CategoryIds { get; set; } = new();
}
