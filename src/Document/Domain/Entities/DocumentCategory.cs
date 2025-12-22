namespace Domain.Entities;

public class DocumentCategory
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid CategoryId { get; set; }
    public Document Document { get; set; } = null!;
    public Category Category { get; set; } = null!;
}