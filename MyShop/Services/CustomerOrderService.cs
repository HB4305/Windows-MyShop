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

    /// <summary>
    /// Gets all orders (owner only).
    /// </summary>
    public async Task<List<CustomerOrder>> GetAllOrdersAsync()
        => await _orderRepo.GetAllAsync();

    /// <summary>
    /// Gets orders for a specific sale (sale role only).
    /// </summary>
    public async Task<List<CustomerOrder>> GetOrdersBySellerAsync(int sellerId)
        => await _orderRepo.GetBySellerIdAsync(sellerId);

    public async Task<CustomerOrder?> GetOrderByIdAsync(int id)
        => await _orderRepo.GetByIdAsync(id);

    public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
        => await _detailRepo.GetByOrderIdAsync(orderId);

    /// <summary>
    /// Creates a new order. Automatically assigns seller_id and seller_name.
    /// </summary>
    public async Task<CustomerOrder> CreateOrderAsync(
        CustomerOrder order,
        List<OrderDetail> details,
        int sellerId,
        string sellerName)
    {
        Validate(order);

        order.CreatedAt = DateTimeOffset.UtcNow;
        order.Status = "Pending";
        order.PaymentStatus = "Unpaid";
        order.TotalAmount = details.Sum(d => d.Quantity * d.UnitPrice);

        var createdOrder = await _orderRepo.CreateAsync(order, sellerId, sellerName);

        foreach (var detail in details)
            detail.OrderId = createdOrder.Id;

        await _detailRepo.CreateBulkAsync(details);

        return createdOrder;
    }

    public async Task UpdateOrderAsync(CustomerOrder order, List<OrderDetail>? newDetails = null)
    {
        Validate(order);

        if (newDetails != null && newDetails.Count > 0)
        {
            order.TotalAmount = newDetails.Sum(d => d.Quantity * d.UnitPrice);

            await _detailRepo.DeleteByOrderIdAsync(order.Id);
            await _detailRepo.CreateBulkAsync(newDetails);
        }

        await _orderRepo.UpdateAsync(order);
    }

    public async Task UpdateStatusAsync(int id, string status)
        => await _orderRepo.UpdateStatusAsync(id, status);

    public async Task UpdatePaymentStatusAsync(int id, string paymentStatus)
        => await _orderRepo.UpdatePaymentStatusAsync(id, paymentStatus);

    public async Task DeleteOrderAsync(int id)
        => await _orderRepo.DeleteAsync(id);

    private void Validate(CustomerOrder order)
    {
        var context = new ValidationContext(order);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(order, context, results, true))
            throw new ValidationException(results.First().ErrorMessage);
    }
}