using Application.DTOs.Page;
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
public class PageController : ControllerBase
{
    private readonly IPageService _pageService;

    public PageController(IPageService pageService)
    {
        _pageService = pageService;
    }

    /// <summary>
    /// Get page by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PageResponse>> GetById(Guid id)
    {

        var result = await _pageService.GetByIdAsync(id);
        return Ok(
            new ApiResponse<PageResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Get all pages with pagination and filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] PageFilterParams filterParams)
    {
        var result = await _pageService.GetAllAsync(filterParams);
        return Ok(
            new ApiResponse<PagedResult<PageResponse>>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Get a specific page by document ID and page number
    /// </summary>
    [HttpGet("document/{documentId}/page/{pageNumber}")]
    public async Task<ActionResult<PageResponse>> GetByDocumentIdAndPageNumber(Guid documentId, int pageNumber)
    {
        var result = await _pageService.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber);

        return Ok(
            new ApiResponse<PageResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    // <summary>
    // Preview a specific page by document ID and page number
    // </summary>
    [HttpGet("document/{documentId}/page/{pageNumber}/preview")]
    public async Task<ActionResult<PagePreviewResponse>> GetPreviewByDocumentIdAndPageNumber(Guid documentId, int pageNumber)
    {
        var result = await _pageService.GetPreviewByDocumentIdAndPageNumberAsync(documentId, pageNumber);

        return Ok(
            new ApiResponse<PagePreviewResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    // <summary>
    // Download content of a specific page by document ID and page number
    // </summary>
    [HttpGet("document/{documentId}/page/{pageNumber}/download")]
    public async Task<ActionResult> DownloadContentByDocumentIdAndPageNumber(Guid documentId, int pageNumber)
    {
        var page = await _pageService.GetByDocumentIdAndPageNumberAsync(documentId, pageNumber);
        var fileContent = await _pageService.DownloadContentAsync(page.Id);

        return File(fileContent.Content, fileContent.ContentType, fileContent.FileName);
    }

    /// <summary>
    /// Create a new page
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PageResponse>> Create([FromForm] PageCreateRequest request)
    {
        var userId = User.GetUserId();
        var role = User.GetUserRole();
        var result = await _pageService.CreateAsync(userId, role, request);
        return Ok(
            new ApiResponse<PageResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Update an existing page
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PageResponse>> Update(Guid id, [FromForm] PageUpdateRequest request)
    {
        var userId = User.GetUserId();
        var role = User.GetUserRole();
        var result = await _pageService.UpdateAsync(id, userId, role, request);
        return Ok(
            new ApiResponse<PageResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Delete a page
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        var role = User.GetUserRole();
        await _pageService.DeleteAsync(id, userId, role);
        return Ok(
            new ApiResponse
            {
                Success = true,
                Message = "Page deleted successfully"
            }
        );
    }
}
