using Application.DTOs.Document;
using Application.DTOs;
using Shared.DTOs;
using Application.Interfaces;
using Domain.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id)
    {
        var result = await _documentService.GetByIdAsync(id);
        return Ok(
            new ApiResponse<DocumentResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Get document by ID with full details (categories, pages)
    /// </summary>
    [HttpGet("{id}/details")]
    public async Task<ActionResult<DocumentResponse>> GetByIdWithDetails(Guid id)
    {
        var result = await _documentService.GetByIdWithDetailsAsync(id);
        return Ok(
            new ApiResponse<DocumentResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Get all documents with pagination and filters
    /// Example: GET /api/document?PageNumber=1&PageSize=10&UserId=123&Title=example&Topic=science&SortBy=Title&IsDescending=false
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] Guid? userId,
        [FromQuery] string? keyword,
        [FromQuery] string? title,
        [FromQuery] string? topic,
        [FromQuery] List<string>? categories,
        [FromQuery] DateTime? createdAfter,
        [FromQuery] DateTime? createdBefore,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool isDescending = true)
    {
        var filterParams = new DocumentFilterParams
        {
            UserId = userId,
            Keyword = keyword,
            Title = title,
            Topic = topic,
            Categories = categories,
            CreatedAfter = createdAfter,
            CreatedBefore = createdBefore,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            IsDescending = isDescending
        };

        var result = await _documentService.GetAllAsync(filterParams);
        return Ok(
            new ApiResponse<PagedResult<DocumentResponse>>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Create a new document
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DocumentResponse>> Create([FromBody] DocumentCreateRequest request)
    {

        var userId = User.GetUserId();
        var result = await _documentService.CreateAsync(userId, request);
        return Ok(
            new ApiResponse<DocumentResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Update an existing document
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DocumentResponse>> Update(Guid id, [FromBody] DocumentUpdateRequest request)
    {

        var userId = User.GetUserId();
        var role = User.GetUserRole();
        var result = await _documentService.UpdateAsync(id, userId, role, request);
        return Ok(
            new ApiResponse<DocumentResponse>
            {
                Success = true,
                Data = result
            }
        );

    }

    /// <summary>
    /// Delete a document (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var user = User.GetUserId();
        var role = User.GetUserRole();
        await _documentService.DeleteAsync(id, user, role);
        return Ok(
            new ApiResponse<string>
            {
                Success = true,
                Data = "Document deleted successfully."
            }
        );
    }

    /// <summary>
    /// Check if a document exists
    /// </summary>
    [HttpGet("exists/{id}")]
    public async Task<ActionResult<bool>> Exists(Guid id)
    {
        var exists = await _documentService.ExistsAsync(id);
        return Ok(
            new ApiResponse<bool>
            {
                Success = true,
                Data = exists
            }
        );
    }
}
