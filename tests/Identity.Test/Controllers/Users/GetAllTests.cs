using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Domain.Filters;

namespace Identity.Test.Controllers.Users;

public class GetAllTests : UsersControllerBase
{
    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenCalled()
    {
        // Arrange
        var filterParams = new UserFilterParams();
        var pagedResult = new PagedResult<UserResponse>
        {
            Items = new List<UserResponse>(),
            TotalCount = 0
        };

        _userService.GetAllAsync(filterParams).Returns(pagedResult);

        // Act
        var result = await _controller.GetAll(filterParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<PagedResult<UserResponse>>>(okResult.Value);
        Assert.Equal(pagedResult, apiResponse.Data);
    }
}
