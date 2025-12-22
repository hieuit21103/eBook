using Application.DTOs.Page;
using Application.DTOs;
using Shared.DTOs;
using Domain.Filters;
using FileStorage.Protos;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IPageService
{
    Task<PageResponse> GetByIdAsync(Guid id);
    Task<PagedResult<PageResponse>> GetAllAsync(PageFilterParams filterParams);
    Task<IEnumerable<PageResponse>> GetPagesByDocumentIdAsync(Guid documentId, PageFilterParams filterParams);
    Task<PageResponse> GetByDocumentIdAndPageNumberAsync(Guid documentId, int pageNumber);
    Task<PageDownloadResponse> DownloadContentAsync(Guid id);
    Task<PagePreviewResponse> GetPreviewByDocumentIdAndPageNumberAsync(Guid documentId, int pageNumber);
    Task<PageResponse> CreateAsync(Guid userId, string role, PageCreateRequest request);
    Task<PageResponse> UpdateAsync(Guid id, Guid userId, string role, PageUpdateRequest request);
    Task DeleteAsync(Guid id, Guid userId, string role);
}
