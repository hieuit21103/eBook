using Microsoft.AspNetCore.Mvc;

namespace Identity.Test.Controllers.Auth;

public class RegisterTests : AuthControllerBase
{
    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "testuser", "password");
        var authResponse = new AuthResponse("accessToken", "refreshToken", Guid.NewGuid(), "testuser", "test@example.com", "User");

        _authService.RegisterAsync(request).Returns(authResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Required");
        var request = new RegisterRequest("", "testuser", "password");

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
