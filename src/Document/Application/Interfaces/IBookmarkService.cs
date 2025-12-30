using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Bookmark;
using Domain.Entities;
using Domain.Filters;

namespace Application.Interfaces;

public interface IBookmarkService
{
    Task<BookmarkResponse> AddBookmarkAsync(string username, BookmarkCreateRequest request);
    Task RemoveBookmarkAsync(Guid pageId, Guid userId);
    Task<bool> IsBookmarkedAsync(Guid pageId, Guid userId);
    Task<PagedResult<BookmarkResponse>> GetBookmarkedDocumentsAsync(Guid userId, string username, BookmarkFilterParams filterParams);
    Task<PagedResult<BookmarkResponse>> GetAllBookmarksAsync(BookmarkFilterParams filterParams);
}