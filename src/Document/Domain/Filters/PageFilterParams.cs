namespace Domain.Filters;

public class PageFilterParams : FilterParams
{
    public Guid? DocumentId { get; set; }
    public int? SpecificPageNumber { get; set; }
}
