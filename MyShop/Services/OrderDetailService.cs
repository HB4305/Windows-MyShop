using MyShop.Models;
using MyShop.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Services;

public class OrderDetailService
{
    private readonly OrderDetailRepository _repository;

    public OrderDetailService(OrderDetailRepository repository) => _repository = repository;
    
    public async Task<List<OrderDetail>> GetAllDetailsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<List<OrderDetail>> GetDetailsByOrderIdAsync(int orderId)
    {
        return await _repository.GetByOrderIdAsync(orderId);
    }

    public async Task<OrderDetail> CreateDetailAsync(OrderDetail detail)
    {
        Validate(detail);
        return await _repository.CreateAsync(detail);
    }

    public async Task UpdateDetailAsync(OrderDetail detail)
    {
        Validate(detail);
        await _repository.UpdateAsync(detail);
    }

    public async Task DeleteDetailAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private void Validate(OrderDetail detail)
    {
        var context = new ValidationContext(detail);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(detail, context, results, true))
            throw new ValidationException(results.First().ErrorMessage);
    }
}