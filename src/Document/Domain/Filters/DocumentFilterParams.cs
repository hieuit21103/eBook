namespace Domain.Filters;

public class DocumentFilterParams : FilterParams
{
    public Guid? UserId { get; set; }
    public string? Title { get; set; }
    public string? Topic { get; set; }
    public IEnumerable<string>? Categories { get; set; }
}
