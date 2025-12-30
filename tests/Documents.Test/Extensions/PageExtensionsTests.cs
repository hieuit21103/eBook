using Domain.Entities;
using Domain.Filters;
using Extensions;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Documents.Test.Extensions;

public class PageExtensionsTests
{
    private readonly ApplicationDbContext _dbContext;

    public PageExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ApplyFilters_WhenDocumentIdProvided_ShouldFilterByDocumentId()
    {
        // Arrange
        var doc1 = new Document { Id = Guid.NewGuid(), Title = "Doc 1" };
        var doc2 = new Document { Id = Guid.NewGuid(), Title = "Doc 2" };
        var page1 = new Page { Id = Guid.NewGuid(), DocumentId = doc1.Id, Document = doc1 };
        var page2 = new Page { Id = Guid.NewGuid(), DocumentId = doc2.Id, Document = doc2 };

        await _dbContext.Documents.AddRangeAsync(doc1, doc2);
        await _dbContext.Pages.AddRangeAsync(page1, page2);
        await _dbContext.SaveChangesAsync();

        var filterParams = new PageFilterParams { DocumentId = doc1.Id };

        // Act
        var query = _dbContext.Pages.AsQueryable().ApplyFilters(filterParams);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == page1.Id);
    }

    [Fact]
    public async Task ApplySort_WhenSortByPageNumberDescending_ShouldSortCorrectly()
    {
        // Arrange
        var page1 = new Page { Id = Guid.NewGuid(), PageNumber = 1 };
        var page2 = new Page { Id = Guid.NewGuid(), PageNumber = 2 };

        await _dbContext.Pages.AddRangeAsync(page1, page2);
        await _dbContext.SaveChangesAsync();

        // Act
        var query = _dbContext.Pages.AsQueryable().ApplySort("pagenumber", true);
        var result = await query.ToListAsync();

        // Assert
        result.First().PageNumber.Should().Be(2);
        result.Last().PageNumber.Should().Be(1);
    }
}
