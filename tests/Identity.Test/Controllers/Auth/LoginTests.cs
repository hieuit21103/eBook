using Microsoft.AspNetCore.Mvc;

namespace Identity.Test.Controllers.Auth;

public class LoginTests : AuthControllerBase
{
    [Fact]
    public async Task Login_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new LoginRequest("testuser", "password");
        var authResponse = new AuthResponse("accessToken", "refreshToken", Guid.NewGuid(), "testuser", "test@example.com", "User");

        _authService.LoginAsync(request).Returns(authResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("Password", "Required");
        var request = new LoginRequest("testuser", "");

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
