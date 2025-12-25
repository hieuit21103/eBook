using Domain.Interfaces;
using Application.Services;
using Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Identity.Test.Services.Users;

public abstract class UserServiceBase
{
    protected readonly IUserRepository _userRepository;
    protected readonly IPasswordService _passwordService;
    protected readonly IPublishEndpoint _publishEndpoint;
    protected readonly ILogger<UserService> _logger;
    protected readonly UserService _userService;

    protected UserServiceBase()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordService = Substitute.For<IPasswordService>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<UserService>>();
        _userService = new UserService(_userRepository, _passwordService, _publishEndpoint, _logger);
    }
}