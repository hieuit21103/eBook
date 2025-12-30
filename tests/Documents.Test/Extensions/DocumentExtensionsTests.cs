using Domain.Entities;
using Domain.Filters;
using Extensions;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Documents.Test.Extensions;

public class DocumentExtensionsTests
{
    private readonly ApplicationDbContext _dbContext;

    public DocumentExtensionsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ApplyFilters_WhenKeywordProvided_ShouldFilterByTitleOrTopic()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document { Id = Guid.NewGuid(), Title = "C# Guide", Topic = "Programming" },
            new Document { Id = Guid.NewGuid(), Title = "Java Guide", Topic = "Programming" },
            new Document { Id = Guid.NewGuid(), Title = "Cooking 101", Topic = "Food" }
        };
        await _dbContext.Documents.AddRangeAsync(documents);
        await _dbContext.SaveChangesAsync();

        var filterParams = new DocumentFilterParams { Keyword = "Guide" };

        // Act
        var query = _dbContext.Documents.AsQueryable().ApplyFilters(filterParams);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Title == "C# Guide");
        result.Should().Contain(d => d.Title == "Java Guide");
    }

    [Fact]
    public async Task ApplyFilters_WhenCategoriesProvided_ShouldFilterByCategories()
    {
        // Arrange
        var category1 = new Category { Id = Guid.NewGuid(), Name = "Tech" };
        var category2 = new Category { Id = Guid.NewGuid(), Name = "Life" };

        var doc1 = new Document { Id = Guid.NewGuid(), Title = "Doc 1" };
        var doc2 = new Document { Id = Guid.NewGuid(), Title = "Doc 2" };

        doc1.Categories = new List<DocumentCategory> { new DocumentCategory { CategoryId = category1.Id, Category = category1 } };
        doc2.Categories = new List<DocumentCategory> { new DocumentCategory { CategoryId = category2.Id, Category = category2 } };

        await _dbContext.Categories.AddRangeAsync(category1, category2);
        await _dbContext.Documents.AddRangeAsync(doc1, doc2);
        await _dbContext.SaveChangesAsync();

        var filterParams = new DocumentFilterParams { Categories = new[] { "Tech" } };

        // Act
        var query = _dbContext.Documents.AsQueryable().ApplyFilters(filterParams);
        var result = await query.ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(d => d.Title == "Doc 1");
    }
}
