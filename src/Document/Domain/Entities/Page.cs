namespace Domain.Entities;

public class Page
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int PageNumber { get; set; }
    public Guid? FileId { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation property
    public Document Document { get; set; } = null!;
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
