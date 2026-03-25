using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Services;

public class CategoryService
{
    private readonly IRepo<Category> _repository;

    public CategoryService(IRepo<Category> repository) => _repository = repository;

    public async Task<List<Category>> GetAllAsync()
    {
        // Có thể thêm logic nghiệp vụ ở đây
        // Ví dụ: lọc, sắp xếp, transform dữ liệu...
        return await _repository.GetAllAsync();
    }
}
