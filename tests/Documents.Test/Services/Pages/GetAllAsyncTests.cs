using Domain.Filters;
using Shared.DTOs;

namespace Documents.Test.Services.Pages;

public class GetAllAsyncTests : PageServiceBase
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnPages_WhenExist()
    {
        // Arrange
        var filterParams = new PageFilterParams { PageNumber = 1, PageSize = 10 };

        var pages = new PagedResult<Page>
        {
            Items = new List<Page> {
                new Page { Id = Guid.NewGuid(), PageNumber = 1, Document = new Document { Title = "Doc A" } },
                new Page { Id = Guid.NewGuid(), PageNumber = 2, Document = new Document { Title = "Doc A" } }
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
