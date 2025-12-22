using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Bookmark;
using Domain.Entities;
using Domain.Filters;

namespace Domain.Interfaces;

public interface IBookmarkRepository : IRepository<Bookmark>
{
    Task<PagedResult<Bookmark>> GetPagedAsync(BookmarkFilterParams filterParams);
}