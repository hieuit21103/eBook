using Domain.Filters;
using Shared.DTOs;

namespace Identity.Test.Services.Users;

public class GetAllAsyncTests : UserServiceBase
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResult_WhenCalled()
    {
        // Arrange
        var filterParams = new UserFilterParams { PageNumber = 1, PageSize = 10 };
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "user1", Email = "user1@test.com", Role = Role.User },
            new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@test.com", Role = Role.Admin }
        };
        var pagedResult = new PagedResult<User>
        {
            Items = users,
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _userRepository.GetAllAsync(filterParams).Returns(pagedResult);

        // Act
        var result = await _userService.GetAllAsync(filterParams);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal("user1", result.Items.First().Username);
    }
}
