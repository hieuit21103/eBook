using Domain.Entities;
using Domain.Filters;
using Extensions;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Documents.Test.Extensions;

public class BookmarkExtensionsTests
{
    private readonly ApplicationDbContext _dbContext;

    public BookmarkExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ApplyFilters_WhenKeywordProvided_ShouldFilterByDocumentTitleOrTopic()
    {
        // Arrange
        var doc1 = new Document { Id = Guid.NewGuid(), Title = "C# Guide", Topic = "Programming" };
        var doc2 = new Document { Id = Guid.NewGuid(), Title = "Cooking", Topic = "Food" };
        var page1 = new Page { Id = Guid.NewGuid(), DocumentId = doc1.Id, Document = doc1 };
        var page2 = new Page { Id = Guid.NewGuid(), DocumentId = doc2.Id, Document = doc2 };

        var bookmark1 = new Bookmark { Id = Guid.NewGuid(), PageId = page1.Id, Page = page1 };
        var bookmark2 = new Bookmark { Id = Guid.NewGuid(), PageId = page2.Id, Page = page2 };

        await _dbContext.Documents.AddRangeAsync(doc1, doc2);
        await _dbContext.Pages.AddRangeAsync(page1, page2);
        await _dbContext.Bookmarks.AddRangeAsync(bookmark1, bookmark2);
        await _dbContext.SaveChangesAsync();

        var filterParams = new BookmarkFilterParams { Keyword = "Guide" };

        // Act
        var query = _dbContext.Bookmarks.AsQueryable().ApplyFilters(filterParams);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(b => b.Id == bookmark1.Id);
    }

    [Fact]
    public async Task ApplyFilters_WhenUserIdProvided_ShouldFilterByUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doc = new Document { Id = Guid.NewGuid(), Title = "Doc", Topic = "Topic" };
        var page = new Page { Id = Guid.NewGuid(), DocumentId = doc.Id, Document = doc };

        var bookmark1 = new Bookmark { Id = Guid.NewGuid(), UserId = userId, PageId = page.Id, Page = page };
        var bookmark2 = new Bookmark { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PageId = page.Id, Page = page };

        await _dbContext.Documents.AddAsync(doc);
        await _dbContext.Pages.AddAsync(page);
        await _dbContext.Bookmarks.AddRangeAsync(bookmark1, bookmark2);
        await _dbContext.SaveChangesAsync();

        var filterParams = new BookmarkFilterParams { UserId = userId };

        // Act
        var query = _dbContext.Bookmarks.AsQueryable().ApplyFilters(filterParams);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(b => b.Id == bookmark1.Id);
    }
}
