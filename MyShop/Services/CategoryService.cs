using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Services;

public class CategoryService
{
    private readonly CategoryRepository _repository;

    public CategoryService(CategoryRepository repository) => _repository = repository;

    public async Task<List<Category>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<int> AddAsync(Category category)
    {
        ValidateCategoryName(category);
        return await _repository.AddAsync(category);
    }

    public async Task UpdateAsync(Category category)
    {
        ValidateCategoryName(category);
        await _repository.UpdateAsync(category);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static void ValidateCategoryName(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            throw new ArgumentException("Category name is required.");
        }

        category.Name = category.Name.Trim();

        if (category.Name.Length > 100)
        {
            throw new ArgumentException("Category name must not exceed 100 characters.");
        }
    }
}
