using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Description { get; set; }
    public int TotalPages { get; set; }
    public bool IsDeleted { get; set; } = false;

    public ICollection<Page> Pages { get; set; } = new List<Page>();
    public ICollection<DocumentCategory> Categories { get; set; } = new List<DocumentCategory>();
}
