using Identity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Controllers.Auth;

public class AuthControllerBase
{
    protected readonly IAuthService _authService;
    protected readonly ILogger<AuthController> _logger;
    protected readonly AuthController _controller;
    protected readonly HttpContext _httpContext;

    public AuthControllerBase()
    {
        _authService = Substitute.For<IAuthService>();
        _logger = Substitute.For<ILogger<AuthController>>();
        _controller = new AuthController(_authService, _logger);
        
        _httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
    }
}
