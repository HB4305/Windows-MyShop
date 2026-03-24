using Microsoft.EntityFrameworkCore;
using MyShop.Api.Data;
using MyShop.Api.DTOs;
using MyShop.Api.Models;

namespace MyShop.Api.Services;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToResponse).ToList();
    }

    private static CategoryResponse MapToResponse(Category c)
        => new(c.Id, c.Name, c.Description);
}
