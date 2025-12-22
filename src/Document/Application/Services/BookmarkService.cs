using Application.Interfaces;
using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Bookmark;
using Domain.Interfaces;
using Domain.Entities;
using Domain.Filters;

namespace Application.Services;

public class BookmarkService : IBookmarkService
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IPageRepository _pageRepository;

    public BookmarkService(
        IBookmarkRepository bookmarkRepository,
        IPageRepository pageRepository)
    {
        _bookmarkRepository = bookmarkRepository;
        _pageRepository = pageRepository;
    }

    public async Task<BookmarkResponse> AddBookmarkAsync(BookmarkCreateRequest request)
    {
        // Check if page exists
        var page = await _pageRepository.GetByIdWithDetailsAsync(request.PageId);
        if (page == null)
        {
            throw new KeyNotFoundException($"Page with ID {request.PageId} not found.");
        }

        // Check if already bookmarked
        var existing = await _bookmarkRepository.FindAsync(b => 
            b.PageId == request.PageId && b.UserId == request.UserId);
        
        if (existing.Any())
        {
            throw new InvalidOperationException("This page is already bookmarked by this user.");
        }

        var bookmark = new Bookmark
        {
            Id = Guid.NewGuid(),
            PageId = request.PageId,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _bookmarkRepository.AddAsync(bookmark);
        
        if (result == null)
        {
            throw new Exception("Failed to add bookmark.");
        }

        return new BookmarkResponse
        {
            Username = "User", // TODO: Get from user service
            DocumentTitle = page.Document?.Title ?? "Unknown",
            DocumentTopic = page.Document?.Topic ?? "Unknown",
            CreatedAt = result.CreatedAt
        };
    }

    public async Task RemoveBookmarkAsync(Guid pageId, Guid userId)
    {
        var bookmark = await _bookmarkRepository.FindAsync(b => 
            b.PageId == pageId && b.UserId == userId);
        
        var bookmarkToDelete = bookmark.FirstOrDefault();
        if (bookmarkToDelete == null)
        {
            throw new KeyNotFoundException("Bookmark not found.");
        }

        await _bookmarkRepository.DeleteAsync(bookmarkToDelete);
    }

    public async Task<bool> IsBookmarkedAsync(Guid pageId, Guid userId)
    {
        return await _bookmarkRepository.ExistsAsync(b => 
            b.PageId == pageId && b.UserId == userId);
    }

    public async Task<PagedResult<BookmarkResponse>> GetBookmarkedDocumentsAsync(Guid userId, BookmarkFilterParams filterParams)
    {
        // Set userId filter
        filterParams.UserId = userId;
        
        var pagedResult = await _bookmarkRepository.GetPagedAsync(filterParams);
        
        var bookmarkResponses = pagedResult.Items.Select(b => new BookmarkResponse
        {
            Username = "User", // TODO: Get from user service
            DocumentTitle = b.Page?.Document?.Title ?? "Unknown",
            DocumentTopic = b.Page?.Document?.Topic ?? "Unknown",
            CreatedAt = b.CreatedAt
        }).ToList();

        return new PagedResult<BookmarkResponse>
        {
            Items = bookmarkResponses,
            CurrentPage = pagedResult.CurrentPage,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<PagedResult<BookmarkResponse>> GetAllBookmarksAsync(BookmarkFilterParams filterParams)
    {
        var pagedResult = await _bookmarkRepository.GetPagedAsync(filterParams);
        
        var bookmarkResponses = pagedResult.Items.Select(b => new BookmarkResponse
        {
            Username = "User", // TODO: Get from user service
            DocumentTitle = b.Page?.Document?.Title ?? "Unknown",
            DocumentTopic = b.Page?.Document?.Topic ?? "Unknown",
            CreatedAt = b.CreatedAt
        }).ToList();

        return new PagedResult<BookmarkResponse>
        {
            Items = bookmarkResponses,
            CurrentPage = pagedResult.CurrentPage,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            TotalCount = pagedResult.TotalCount
        };
    }
}