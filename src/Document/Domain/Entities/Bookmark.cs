namespace Domain.Entities;

public class Bookmark
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PageId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Page Page { get; set; } = null!;
}
