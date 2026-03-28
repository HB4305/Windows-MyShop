using MyShop.Models;
using Supabase;

namespace MyShop.Repositories;

public class CustomerOrderRepository
{
    private readonly Supabase.Client _client;

    public CustomerOrderRepository(Supabase.Client client) => _client = client;

    public async Task<List<CustomerOrder>> GetAllAsync()
    {
        var result = await _client.From<CustomerOrder>().Get();
        return result.Models;
    }

    public async Task<CustomerOrder?> GetByIdAsync(int id)
    {
        var result = await _client.From<CustomerOrder>()
            .Where(o => o.Id == id)
            .Single();
        return result;
    }
    
    public async Task<CustomerOrder> CreateAsync(CustomerOrder order)
    {
        var result = await _client.From<CustomerOrder>().Insert(order);
        return result.Models.First();
    }

    public async Task<CustomerOrder> UpdateAsync(CustomerOrder order)
    {
        var result = await _client.From<CustomerOrder>().Update(order);
        return result.Models.First();
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        await _client.From<CustomerOrder>()
            .Where(o => o.Id == id)
            .Set(o => o.Status, status)
            .Update();
    }

    public async Task UpdatePaymentStatusAsync(int id, string paymentStatus)
    {
        await _client.From<CustomerOrder>()
            .Where(o => o.Id == id)
            .Set(o => o.PaymentStatus, paymentStatus)
            .Update();
    }

    public async Task DeleteAsync(int id)
    {
        await _client.From<CustomerOrder>()
            .Where(o => o.Id == id)
            .Delete();
    }
}