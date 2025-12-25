namespace Identity.Test.Services.Auth;

public abstract class AuthServiceBase
{
    protected readonly IUserRepository _userRepository;
    protected readonly ITokenService _tokenService;
    protected readonly IPasswordService _passwordService;
    protected readonly AuthService _authService;

    protected AuthServiceBase()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _passwordService = Substitute.For<IPasswordService>();
        _authService = new AuthService(_userRepository, _tokenService, _passwordService);
    }
}