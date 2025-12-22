using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Bookmark;
using Application.Interfaces;
using Domain.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookmarkController : ControllerBase
{
    private readonly IBookmarkService _bookmarkService;

    public BookmarkController(IBookmarkService bookmarkService)
    {
        _bookmarkService = bookmarkService;
    }

    /// <summary>
    /// Add a new bookmark
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BookmarkResponse>> AddBookmark([FromBody] BookmarkCreateRequest request)
    {
        var result = await _bookmarkService.AddBookmarkAsync(request);
        return Ok(
            new ApiResponse<BookmarkResponse>
            {
                Success = true,
                Message = "Bookmark added successfully",
                Data = result,
            }
        );
    }

    /// <summary>
    /// Remove a bookmark
    /// </summary>
    [HttpDelete("{pageId}/{userId}")]
    public async Task<ActionResult> RemoveBookmark(Guid pageId, Guid userId)
    {
        await _bookmarkService.RemoveBookmarkAsync(pageId, userId);
        return Ok(
            new ApiResponse
            {
                Message = "Bookmark removed successfully",
                Success = true
            }
        );
    }

    /// <summary>
    /// Check if a page is bookmarked by a user
    /// </summary>
    [HttpGet("check/{pageId}/{userId}")]
    public async Task<ActionResult<bool>> CheckBookmark(Guid pageId, Guid userId)
    {
        var isBookmarked = await _bookmarkService.IsBookmarkedAsync(pageId, userId);
        return Ok(
            new ApiResponse<bool>
            {
                Success = true,
                Data = isBookmarked
            }
        );
    }

    /// <summary>
    /// Get bookmarked documents for a specific user with pagination and filters
    /// Example: GET /api/bookmark/user/123?PageNumber=1&PageSize=10&SortBy=CreatedAt&IsDescending=true&DocumentTitle=example
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult> GetUserBookmarks(
        Guid userId,
        [FromQuery] string? keyword,
        [FromQuery] string? documentTitle,
        [FromQuery] string? documentTopic,
        [FromQuery] DateTime? createdAfter,
        [FromQuery] DateTime? createdBefore,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool isDescending = true)
    {
        var filterParams = new BookmarkFilterParams
        {
            Keyword = keyword,
            DocumentTitle = documentTitle,
            DocumentTopic = documentTopic,
            CreatedAfter = createdAfter,
            CreatedBefore = createdBefore,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            IsDescending = isDescending
        };

        var result = await _bookmarkService.GetBookmarkedDocumentsAsync(userId, filterParams);
        return Ok(
            new ApiResponse<PagedResult<BookmarkResponse>>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Get all bookmarks with pagination and filters (Admin endpoint)
    /// Example: GET /api/bookmark?PageNumber=1&PageSize=10&UserId=123&DocumentTitle=example&SortBy=CreatedAt&IsDescending=true
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAllBookmarks(
        [FromQuery] Guid? userId,
        [FromQuery] string? keyword,
        [FromQuery] string? documentTitle,
        [FromQuery] string? documentTopic,
        [FromQuery] DateTime? createdAfter,
        [FromQuery] DateTime? createdBefore,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool isDescending = true)
    {
        var filterParams = new BookmarkFilterParams
        {
            UserId = userId,
            Keyword = keyword,
            DocumentTitle = documentTitle,
            DocumentTopic = documentTopic,
            CreatedAfter = createdAfter,
            CreatedBefore = createdBefore,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            IsDescending = isDescending
        };

        var result = await _bookmarkService.GetAllBookmarksAsync(filterParams);
        return Ok(result);
    }
}
