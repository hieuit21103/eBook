using Domain.Entities;
using Domain.Filters;
using Extensions;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Documents.Test.Extensions;

public class CategoryExtensionsTests
{
    private readonly ApplicationDbContext _dbContext;

    public CategoryExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ApplyFilters_WhenKeywordProvided_ShouldFilterByName()
    {
        // Arrange
        var cat1 = new Category { Id = Guid.NewGuid(), Name = "Technology" };
        var cat2 = new Category { Id = Guid.NewGuid(), Name = "Science" };

        await _dbContext.Categories.AddRangeAsync(cat1, cat2);
        await _dbContext.SaveChangesAsync();

        var filterParams = new CategoryFilterParams { Keyword = "Tech" };

        // Act
        var query = _dbContext.Categories.AsQueryable().ApplyFilters(filterParams);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(c => c.Name == "Technology");
    }

    [Fact]
    public async Task ApplySort_WhenSortByNameDescending_ShouldSortCorrectly()
    {
        // Arrange
        var cat1 = new Category { Id = Guid.NewGuid(), Name = "A" };
        var cat2 = new Category { Id = Guid.NewGuid(), Name = "B" };

        await _dbContext.Categories.AddRangeAsync(cat1, cat2);
        await _dbContext.SaveChangesAsync();

        // Act
        var query = _dbContext.Categories.AsQueryable().ApplySort("name", true);
        var result = await query.ToListAsync();

        // Assert
        result.First().Name.Should().Be("B");
        result.Last().Name.Should().Be("A");
    }
}
