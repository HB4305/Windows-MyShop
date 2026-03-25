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
}
