using Application.DTOs.Category;
using Application.DTOs;
using Shared.DTOs;
using Application.Interfaces;
using Domain.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return Ok(
            new ApiResponse<CategoryResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Get all categories with pagination and filters
    /// Example: GET /api/category?PageNumber=1&PageSize=10&Name=Fiction&SortBy=Name&IsDescending=false
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] CategoryFilterParams filterParams)
    {
        var result = await _categoryService.GetAllAsync(filterParams);
        return Ok(
            new ApiResponse<PagedResult<CategoryResponse>>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create([FromBody] CategoryCreateRequest request)
    {

        var result = await _categoryService.CreateAsync(request);
        return Ok(
            new ApiResponse<CategoryResponse>
            {
                Success = true,
                Data = result
            }
        );
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryResponse>> Update(Guid id, [FromBody] CategoryUpdateRequest request)
    {

        var result = await _categoryService.UpdateAsync(id, request);
        return Ok(
            new ApiResponse<CategoryResponse>
            {
                Success = true,
                Data = result
            }
        );

    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteAsync(id);
        return Ok(
            new ApiResponse<string>
            {
                Success = true,
                Data = "Category deleted successfully."
            }
        );
    }

    /// <summary>
    /// Check if a category exists
    /// </summary>
    [HttpGet("exists/{id}")]
    public async Task<ActionResult<bool>> Exists(Guid id)
    {
        var exists = await _categoryService.ExistsAsync(id);
        return Ok(
            new ApiResponse<bool>
            {
                Success = true,
                Data = exists
            }
        );
    }
}
