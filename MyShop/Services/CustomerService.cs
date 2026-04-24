using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Services;

public class CustomerService
{
    private readonly CustomerRepository _repository;

    public CustomerService(CustomerRepository repository) => _repository = repository;

    public Task<(List<Customer> Items, int TotalCount)> GetCustomersAsync(int page, int pageSize, string keyword)
        => _repository.GetItemsAsync(page, pageSize, keyword);

    public Task<Customer?> GetCustomerByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public Task<Customer?> GetCustomerByPhoneAsync(string phone)
        => _repository.GetByPhoneAsync(phone);

    public async Task<int> SaveCustomerAsync(Customer customer)
    {
        if (customer.Id == 0)
        {
            return await _repository.CreateAsync(customer);
        }
        else
        {
            await _repository.UpdateAsync(customer);
            return customer.Id;
        }
    }

    public Task DeleteCustomerAsync(int id)
        => _repository.DeleteAsync(id);

    public Task<List<CustomerOrder>> GetCustomerOrderHistoryAsync(int customerId)
        => _repository.GetOrderHistoryAsync(customerId);
}
