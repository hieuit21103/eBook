namespace Domain.Filters;

public class BookmarkFilterParams : FilterParams
{
    public Guid? UserId { get; set; }
    public string? DocumentTitle { get; set; }
    public string? DocumentTopic { get; set; }
}
