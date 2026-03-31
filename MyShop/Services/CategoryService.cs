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
}
