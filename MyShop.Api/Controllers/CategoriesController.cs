using Microsoft.AspNetCore.Mvc;
using MyShop.Api.DTOs;
using MyShop.Api.Services;

namespace MyShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary> Lấy danh sách tất cả danh mục </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryResponse>>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(new ApiResponse<List<CategoryResponse>>(true, "Lấy danh sách danh mục thành công", categories));
    }
}
