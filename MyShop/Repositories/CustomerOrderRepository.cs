using MyShop.Models;

namespace MyShop.Repositories;

public class CustomerOrderRepository
{
    private readonly Supabase.Client _client;

    public CustomerOrderRepository(Supabase.Client client) => _client = client;

    public async Task<List<CustomerOrder>> GetAllAsync()
    {
        var response = await _client.From<CustomerOrder>().Get();
        return response.Models;
    }
}