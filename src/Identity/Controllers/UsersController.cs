using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Domain.Filters;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserFilterParams filterParams)
    {
        var users = await _userService.GetAllAsync(filterParams);
        return Ok(ApiResponse<PagedResult<UserResponse>>.Ok(users));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound(ApiResponse.Fail("User not found"));
        return Ok(ApiResponse<UserResponse>.Ok(user));
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateRequest request)
    {
        var user = await _userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ApiResponse<UserResponse>.Ok(user, "User created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UserUpdateRequest request)
    {
        var user = await _userService.UpdateAsync(id, request);
        if (user == null) return NotFound(ApiResponse.Fail("User not found"));
        return Ok(ApiResponse<UserResponse>.Ok(user, "User updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result) return NotFound(ApiResponse.Fail("User not found"));
        return Ok(ApiResponse.SuccessResponse("User deleted successfully"));
    }
}
