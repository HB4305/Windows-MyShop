using MyShop.Models;

namespace MyShop.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
}
