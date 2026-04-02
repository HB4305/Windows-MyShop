using MyShop.Models;
using MyShop.Repositories;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Services;

public class CustomerOrderService
{
    private readonly CustomerOrderRepository _orderRepo;
    private readonly OrderDetailRepository _detailRepo;

    public CustomerOrderService(CustomerOrderRepository orderRepo, OrderDetailRepository detailRepo)
    {
        _orderRepo = orderRepo;
        _detailRepo = detailRepo;
    }

    public async Task<List<CustomerOrder>> GetAllOrdersAsync()
    {
        return await _orderRepo.GetAllAsync();
    }

    public async Task<CustomerOrder?> GetOrderByIdAsync(int id)
    {
        return await _orderRepo.GetByIdAsync(id);
    }

    public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
    {
        return await _detailRepo.GetByOrderIdAsync(orderId);
    }
    
    public async Task<CustomerOrder> CreateOrderAsync(CustomerOrder order, List<OrderDetail> details)
    {
        Validate(order);

        order.CreatedAt = DateTimeOffset.UtcNow;
        order.Status = "Pending";
        order.PaymentStatus = "Unpaid";
        order.TotalAmount = details.Sum(d => d.Quantity * d.UnitPrice);

        var createdOrder = await _orderRepo.CreateAsync(order);

        foreach (var detail in details)
            detail.OrderId = createdOrder.Id;

        await _detailRepo.CreateBulkAsync(details);

        return createdOrder;
    }

    public async Task UpdateOrderAsync(CustomerOrder order, List<OrderDetail>? newDetails = null)
    {
        Validate(order);

        if (newDetails != null)
        {
            order.TotalAmount = newDetails.Sum(d => d.Quantity * d.UnitPrice);

            await _detailRepo.DeleteByOrderIdAsync(order.Id);
            await _detailRepo.CreateBulkAsync(newDetails);
        }

        await _orderRepo.UpdateAsync(order);
    }

    public async Task DeleteOrderAsync(int id)
    {
        // OrderDetails auto deleted via CASCADE
        await _orderRepo.DeleteAsync(id);
    }

    private void Validate(CustomerOrder order)
    {
        var context = new ValidationContext(order);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(order, context, results, true))
            throw new ValidationException(results.First().ErrorMessage);
    }
}