using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Identity.Test.Controllers.Users;

public class CreateTests : UsersControllerBase
{
    [Fact]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new UserCreateRequest { Username = "test", Email = "test@test.com", Password = "password" };
        var userResponse = new UserResponse { Id = Guid.NewGuid(), Username = "test" };

        _userService.CreateAsync(request).Returns(userResponse);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UserResponse>>(createdResult.Value);
        Assert.Equal(userResponse, apiResponse.Data);
    }
}
