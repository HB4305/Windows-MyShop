using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository) => _repository = repository;

    public async Task<List<Category>> GetAllAsync()
    {
        // Có thể thêm logic nghiệp vụ ở đây
        // Ví dụ: lọc, sắp xếp, transform dữ liệu...
        return await _repository.GetAllAsync();
    }
}
