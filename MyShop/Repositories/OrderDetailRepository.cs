using MyShop.Models;
using Supabase;

namespace MyShop.Repositories;

public class OrderDetailRepository
{
    private readonly Supabase.Client _client;
    public OrderDetailRepository(Supabase.Client client) => _client = client;

    public async Task<List<OrderDetail>> GetAllAsync()
    {
        var result = await _client.From<OrderDetail>().Get();
        return result.Models;
    }

    public async Task<List<OrderDetail>> GetByOrderIdAsync(int orderId)
    {
        var result = await _client.From<OrderDetail>()
            .Where(d => d.OrderId == orderId)
            .Get();
        return result.Models;
    }

    public async Task<OrderDetail> CreateAsync(OrderDetail detail)
    {
        var result = await _client.From<OrderDetail>().Insert(detail);
        return result.Models.First();
    }

    public async Task CreateBulkAsync(List<OrderDetail> details)
    {
        await _client.From<OrderDetail>().Insert(details);
    }

    public async Task<OrderDetail> UpdateAsync(OrderDetail detail)
    {
        var result = await _client.From<OrderDetail>().Update(detail);
        return result.Models.First();
    }

    public async Task DeleteByOrderIdAsync(int orderId)
    {
        await _client.From<OrderDetail>()
            .Where(d => d.OrderId == orderId)
            .Delete();
    }

    public async Task DeleteAsync(int id)
    {
        await _client.From<OrderDetail>()
            .Where(d => d.Id == id)
            .Delete();
    }
}