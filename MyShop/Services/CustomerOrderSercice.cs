using MyShop.Models;
using MyShop.Models.ControlModels;
using MyShop.Repositories;

namespace MyShop.Services;

public class CustomerOrderService
{
    private readonly CustomerOrderRepository _repository;

    public CustomerOrderService(CustomerOrderRepository repository) => _repository = repository;

    public async Task<List<CustomerOrder>> GetAllAsync()
    {
        // Có thể thêm logic nghiệp vụ ở đây
        // Ví dụ: lọc, sắp xếp, transform dữ liệu...
        return await _repository.GetAllAsync();
    }

    public async Task<List<SaleMonthlyChart>> GetSaleMonthlyChartAsync()
    {
        // Lấy dữ liệu từ repository (đã group theo ngày)
        var data = await _repository.GetSaleMonthlyChartAsync();

        var today = DateTime.Now;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        // Lọc dữ liệu trong tháng hiện tại
        var currentMonthData = data
            .Where(x => x.Date >= firstDayOfMonth && x.Date <= lastDayOfMonth)
            .ToDictionary(x => x.Date, x => x.Revenue);

        var fullMonthData = new List<SaleMonthlyChart>();

        // Điền các ngày bị thiếu bằng 0
        for (var date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
        {
            fullMonthData.Add(new SaleMonthlyChart
            {
                Date = date,
                Revenue = currentMonthData.ContainsKey(date) ? currentMonthData[date] : 0
            });
        }

        // Nếu dữ liệu trống hoàn toàn (database chưa có order), fallback về dạng mock data để demo trên chart
        if (!fullMonthData.Any(x => x.Revenue > 0))
        {
            var random = new Random();
            decimal previousRevenue = 500;
            foreach (var item in fullMonthData)
            {
                previousRevenue += random.Next(-100, 250);
                if (previousRevenue < 0) previousRevenue = 0;
                item.Revenue = previousRevenue;
            }
        }

        return fullMonthData;
    }
}