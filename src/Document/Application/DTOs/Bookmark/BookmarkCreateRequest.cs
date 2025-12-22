namespace Application.DTOs.Bookmark;

public class BookmarkCreateRequest
{
    public Guid PageId { get; set; }
    public Guid UserId { get; set; }
}