using Domain.Filters;
using Shared.DTOs;

namespace Documents.Test.Services.Pages;

public class GetAllAsyncTests : PageServiceBase
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnPages_WhenExist()
    {
        // Arrange
        var document1 = new Document { Id = Guid.NewGuid(), Title = "Doc A" };
        var document2 = new Document { Id = Guid.NewGuid(), Title = "Doc B" };
        var filterParams = new PageFilterParams { 
            DocumentId = document1.Id,
            SpecificPageNumber = 1,
            PageNumber = 1, 
            PageSize = 10 };

        var pages = new PagedResult<Page>
        {
            Items = new List<Page> {
                new Page { Id = Guid.NewGuid(), PageNumber = 1, Document = document1 },
                new Page { Id = Guid.NewGuid(), PageNumber = 2, Document = document2 }
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _pageRepository.GetPagedAsync(filterParams).Returns(pages);

        // Act
        var result = await _pageService.GetAllAsync(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.First().PageNumber.Should().Be(1);
    }
}
