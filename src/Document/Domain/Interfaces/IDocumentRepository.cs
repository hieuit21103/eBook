using Application.DTOs;
using Shared.DTOs;
using Domain.Entities;
using Domain.Filters;

namespace Domain.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<PagedResult<Document>> GetPagedAsync(DocumentFilterParams filterParams);
    Task<Document?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId);
}
