using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Identity.Test.Controllers.Users;

public class DeleteTests : UsersControllerBase
{
    [Fact]
    public async Task Delete_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userService.DeleteAsync(userId).Returns(true);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userService.DeleteAsync(userId).Returns(false);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
