namespace Application.DTOs.Bookmark;

public class BookmarkResponse
{
    public string Username { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = string.Empty;
    public string DocumentTopic { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}