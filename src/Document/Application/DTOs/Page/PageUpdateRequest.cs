namespace Application.DTOs.Page;

public class PageUpdateRequest
{
    public int PageNumber { get; set; }
    public IFormFile? Content { get; set; }
}
