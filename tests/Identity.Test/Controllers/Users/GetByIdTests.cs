using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Identity.Test.Controllers.Users;

public class GetByIdTests : UsersControllerBase
{
    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userResponse = new UserResponse { Id = userId, Username = "test" };

        _userService.GetByIdAsync(userId).Returns(userResponse);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UserResponse>>(okResult.Value);
        Assert.Equal(userResponse, apiResponse.Data);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userService.GetByIdAsync(userId).Returns((UserResponse?)null);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
