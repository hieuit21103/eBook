using Controllers;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Domain.Filters;

namespace Identity.Test.Controllers.Users;

public class UsersControllerBase
{
    protected readonly IUserService _userService;
    protected readonly UsersController _controller;

    public UsersControllerBase()
    {
        _userService = Substitute.For<IUserService>();
        _controller = new UsersController(_userService);
    }
}
