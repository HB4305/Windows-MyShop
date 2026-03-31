using MyShop.Models;

namespace MyShop.Repositories;

public class CategoryRepository
{
    private readonly Supabase.Client _client;

    public CategoryRepository(Supabase.Client client) => _client = client;

    public async Task<List<Category>> GetAllAsync()
    {
        var response = await _client.From<Category>().Get();
        return response.Models;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var response = await _client.From<Category>()
            .Where(x => x.Id == id)
            .Get();

        return response.Models.FirstOrDefault();
    }

    public async Task<int> AddAsync(Category category)
    {
        var result = await _client.From<Category>().Insert(category);
        return result.Models.FirstOrDefault()?.Id ?? 0;
    }

    public async Task UpdateAsync(Category category)
    {
        await _client.From<Category>().Update(category);
    }

    public async Task DeleteAsync(int id)
    {
        await _client.From<Category>()
            .Where(x => x.Id == id)
            .Delete();
    }
}
