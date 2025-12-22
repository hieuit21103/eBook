using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Document;
using Application.Interfaces;
using Domain.Entities;
using Domain.Filters;
using Domain.Interfaces;

namespace Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IDocumentCategoryRepository _documentCategoryRepository;

    public DocumentService(
        IDocumentRepository documentRepository, 
        ICategoryRepository categoryRepository,
        IDocumentCategoryRepository documentCategoryRepository)
    {
        _documentRepository = documentRepository;
        _categoryRepository = categoryRepository;
        _documentCategoryRepository = documentCategoryRepository;
    }

    public async Task<DocumentResponse> GetByIdAsync(Guid id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        
        if (document == null || document.IsDeleted)
        {
            throw new KeyNotFoundException($"Document with ID {id} not found.");
        }

        return MapToResponse(document);
    }

    public async Task<DocumentResponse> GetByIdWithDetailsAsync(Guid id)
    {
        var document = await _documentRepository.GetByIdWithDetailsAsync(id);
        
        if (document == null)
        {
            throw new KeyNotFoundException($"Document with ID {id} not found.");
        }

        return MapToResponseWithDetails(document);
    }

    public async Task<PagedResult<DocumentResponse>> GetAllAsync(DocumentFilterParams filterParams)
    {
        var pagedResult = await _documentRepository.GetPagedAsync(filterParams);
        
        var documentResponses = pagedResult.Items.Select(MapToResponseWithDetails).ToList();

        return new PagedResult<DocumentResponse>
        {
            Items = documentResponses,
            CurrentPage = pagedResult.CurrentPage,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<DocumentResponse> CreateAsync(Guid userId, DocumentCreateRequest request)
    {
        // Validate categories exist
        if (request.CategoryIds.Any())
        {
            foreach (var categoryId in request.CategoryIds)
            {
                if (!await _categoryRepository.ExistsAsync(c => c.Id == categoryId))
                {
                    throw new InvalidOperationException($"Category with ID {categoryId} not found.");
                }
            }
        }

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Topic = request.Topic,
            Description = request.Description,
            TotalPages = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var result = await _documentRepository.AddAsync(document);

        // Add categories
        if (request.CategoryIds.Any())
        {
            var documentCategories = request.CategoryIds.Select(categoryId => new DocumentCategory
            {
                Id = Guid.NewGuid(),
                DocumentId = result.Id,
                CategoryId = categoryId
            });

            await _documentCategoryRepository.AddRangeAsync(documentCategories);
        }

        return await GetByIdWithDetailsAsync(result.Id);
    }

    public async Task<DocumentResponse> UpdateAsync(Guid id, Guid userId, string role, DocumentUpdateRequest request)
    {
        var document = await _documentRepository.GetByIdWithDetailsAsync(id);
        
        if (document == null)
        {
            throw new KeyNotFoundException($"Document with ID {id} not found.");
        }

        if (role != "Admin" && document.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this document.");
        }

        if (request.CategoryIds.Any())
        {
            foreach (var categoryId in request.CategoryIds)
            {
                if (!await _categoryRepository.ExistsAsync(c => c.Id == categoryId))
                {
                    throw new InvalidOperationException($"Category with ID {categoryId} not found.");
                }
            }
        }

        document.Title = request.Title;
        document.Topic = request.Topic;
        document.Description = request.Description;
        document.UpdatedAt = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document);

        await _documentCategoryRepository.DeleteByDocumentIdAsync(id);

        if (request.CategoryIds.Any())
        {
            var documentCategories = request.CategoryIds.Select(categoryId => new DocumentCategory
            {
                Id = Guid.NewGuid(),
                DocumentId = id,
                CategoryId = categoryId
            });

            await _documentCategoryRepository.AddRangeAsync(documentCategories);
        }

        return await GetByIdWithDetailsAsync(id);
    }

    public async Task DeleteAsync(Guid id, Guid userId, string role)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        
        if (document == null || document.IsDeleted)
        {
            throw new KeyNotFoundException($"Document with ID {id} not found.");
        }

        if (role != "Admin" && document.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this document.");
        }

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;
        await _documentRepository.UpdateAsync(document);
    }

    public async Task DeleteDocumentsByUserIdAsync(Guid userId)
    {
        var documents = await _documentRepository.GetByUserIdAsync(userId);
        foreach (var document in documents)
        {
            document.IsDeleted = true;
            document.UpdatedAt = DateTime.UtcNow;
            await _documentRepository.UpdateAsync(document);
        }
    }

    public async Task RestoreDocumentsByUserIdAsync(Guid userId)
    {
        var documents = await _documentRepository.GetByUserIdAsync(userId);
        foreach (var document in documents)
        {
            if (document.IsDeleted)
            {
                document.IsDeleted = false;
                document.UpdatedAt = DateTime.UtcNow;
                await _documentRepository.UpdateAsync(document);
            }
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _documentRepository.ExistsAsync(d => d.Id == id && !d.IsDeleted);
    }

    private static DocumentResponse MapToResponse(Document document)
    {
        return new DocumentResponse
        {
            Id = document.Id,
            UserId = document.UserId,
            Title = document.Title,
            Topic = document.Topic,
            Description = document.Description,
            TotalPages = document.TotalPages,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            Categories = new List<string>(),
        };
    }

    private static DocumentResponse MapToResponseWithDetails(Document document)
    {
        return new DocumentResponse
        {
            Id = document.Id,
            UserId = document.UserId,
            Title = document.Title,
            Topic = document.Topic,
            Description = document.Description,
            TotalPages = document.TotalPages,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            Categories = document.Categories?.Select(dc => dc.Category?.Name ?? "Unknown").ToList() ?? new List<string>(),
        };
    }
}
