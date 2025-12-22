namespace Application.DTOs.Page;

public class PageCreateRequest
{
    public Guid DocumentId { get; set; }
    public int PageNumber { get; set; } = 0;
    public IFormFile Content { get; set; } = null!;
}
