using MyShop.Models;

namespace MyShop.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
}
