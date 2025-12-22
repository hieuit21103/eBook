using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Document;
using Domain.Filters;

namespace Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponse> GetByIdAsync(Guid id);
    Task<DocumentResponse> GetByIdWithDetailsAsync(Guid id);
    Task<PagedResult<DocumentResponse>> GetAllAsync(DocumentFilterParams filterParams);
    Task<DocumentResponse> CreateAsync(Guid userId, DocumentCreateRequest request);
    Task<DocumentResponse> UpdateAsync(Guid id, Guid userId, string role, DocumentUpdateRequest request);
    Task DeleteAsync(Guid id, Guid userId, string role);
    Task DeleteDocumentsByUserIdAsync(Guid userId);
    Task RestoreDocumentsByUserIdAsync(Guid userId);
    Task<bool> ExistsAsync(Guid id);
}
