namespace Application.DTOs.Document;

public class DocumentCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Guid> CategoryIds { get; set; } = new();
}
