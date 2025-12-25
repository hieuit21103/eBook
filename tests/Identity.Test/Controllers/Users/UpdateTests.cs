using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Identity.Test.Controllers.Users;

public class UpdateTests : UsersControllerBase
{
    [Fact]
    public async Task Update_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UserUpdateRequest { Username = "updated" };
        var userResponse = new UserResponse { Id = userId, Username = "updated" };

        _userService.UpdateAsync(userId, request).Returns(userResponse);

        // Act
        var result = await _controller.Update(userId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UserResponse>>(okResult.Value);
        Assert.Equal(userResponse, apiResponse.Data);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UserUpdateRequest();
        _userService.UpdateAsync(userId, request).Returns((UserResponse?)null);

        // Act
        var result = await _controller.Update(userId, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
