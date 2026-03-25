using MyShop.Models;
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
}