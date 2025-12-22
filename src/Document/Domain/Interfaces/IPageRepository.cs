using Domain.Entities;
using Application.DTOs;
using Shared.DTOs;
using Domain.Filters;

namespace Domain.Interfaces;

public interface IPageRepository : IRepository<Page>
{
    Task<PagedResult<Page>> GetPagedAsync(PageFilterParams filterParams);
    Task<IEnumerable<Page>> GetPagesByDocumentIdAsync(Guid documentId);
    Task<Page?> GetByDocumentIdAndPageNumberAsync(Guid documentId, int pageNumber);
    Task<IEnumerable<Page>> GetPagesGreaterThanOrEqualToAsync(Guid documentId, int pageNumber);
    Task<Page?> GetByIdWithDetailsAsync(Guid id);
}
