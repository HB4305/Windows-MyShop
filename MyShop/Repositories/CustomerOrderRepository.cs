using MyShop.Models;
using MyShop.Models.ControlModels;

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

    public async Task<List<SaleMonthlyChart>> GetSaleMonthlyChartAsync()
    {
        // Lấy tất cả Order và tính tổng doanh thu theo ngày
        var response = await _client.From<CustomerOrder>().Get();
        var orders = response.Models;

        var data = orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SaleMonthlyChart
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToList();

        return data;
    }
}