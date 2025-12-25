using Domain.Interfaces;
using Application.Services;
using Domain.Entities;
using NSubstitute;

namespace Documents.Test.Services.Bookmarks;

public abstract class BookmarkServiceBase
{
    protected readonly IBookmarkRepository _bookmarkRepository;
    protected readonly IPageRepository _pageRepository;
    protected readonly BookmarkService _bookmarkService;

    protected BookmarkServiceBase()
    {
        _bookmarkRepository = Substitute.For<IBookmarkRepository>();
        _pageRepository = Substitute.For<IPageRepository>();
        _bookmarkService = new BookmarkService(_bookmarkRepository, _pageRepository);
    }
}
