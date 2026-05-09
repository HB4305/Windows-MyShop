using MyShop.Models;
using MyShop.Repositories;
using System.ComponentModel.DataAnnotations;
using Npgsql;

namespace MyShop.Services;

public class CustomerOrderService
{
    private readonly CustomerOrderRepository _orderRepo;
    private readonly OrderDetailRepository _detailRepo;
    private readonly SportItemRepository _itemRepo;
    private readonly DbConnectionFactory _connFactory;

    public CustomerOrderService(
        CustomerOrderRepository orderRepo,
        OrderDetailRepository detailRepo,
        SportItemRepository itemRepo,
        DbConnectionFactory connFactory)
    {
        _orderRepo = orderRepo;
        _detailRepo = detailRepo;
        _itemRepo = itemRepo;
        _connFactory = connFactory;
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
        {
            detail.OrderId = createdOrder.Id;
            if (detail.VariantId.HasValue)
            {
                await _itemRepo.DeductVariantStockAsync(detail.VariantId.Value, detail.Quantity);
            }
            else if (detail.ItemId.HasValue)
            {
                await _itemRepo.DeductStockAsync(detail.ItemId.Value, detail.Quantity);
            }
        }

        await _detailRepo.CreateBulkAsync(details);

        return createdOrder;
    }

    /// <summary>
    /// POS checkout path with atomic transaction:
    /// 1) Insert customer order
    /// 2) Insert all order details
    /// 3) Deduct stock strictly from sportitem_variants
    /// </summary>
    public async Task<CustomerOrder> CreatePosCheckoutOrderAsync(
        CustomerOrder order,
        List<OrderDetail> details,
        int sellerId,
        string sellerName)
    {
        if (details.Count == 0)
        {
            throw new ValidationException("Order must contain at least one item.");
        }

        Validate(order);

        order.CreatedAt = DateTimeOffset.UtcNow;
        order.Status = "Completed";
        order.PaymentStatus = "Paid";
        order.TotalAmount = details.Sum(d => d.Quantity * d.UnitPrice);

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var createdOrder = await _orderRepo.CreateAsync(order, sellerId, sellerName, conn, tx);

            foreach (var detail in details)
            {
                if (!detail.VariantId.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Item '{detail.ItemName ?? detail.ItemId?.ToString() ?? "Unknown"}' is missing a variant.");
                }

                detail.OrderId = createdOrder.Id;

                await DeductVariantStockAsync(conn, tx, detail.VariantId.Value, detail.Quantity);
                await InsertOrderDetailAsync(conn, tx, detail);
            }

            await tx.CommitAsync();
            return createdOrder;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
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

    private static async Task DeductVariantStockAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        long variantId,
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        const string sql = @"
            UPDATE sportitem_variants
            SET stock_quantity = stock_quantity - @quantity
            WHERE id = @variantId
              AND stock_quantity >= @quantity";

        await using var cmd = new NpgsqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("variantId", variantId);
        cmd.Parameters.AddWithValue("quantity", quantity);
        var affected = await cmd.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException("Not enough stock for the selected variant.");
        }
    }

    private static async Task InsertOrderDetailAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        OrderDetail detail)
    {
        const string sql = @"
            INSERT INTO orderdetails (order_id, item_id, item_name, quantity, unit_price, variant_id, size, color)
            VALUES (@orderId, @itemId, @itemName, @quantity, @unitPrice, @variantId, @size, @color)";

        await using var cmd = new NpgsqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("orderId", detail.OrderId ?? throw new InvalidOperationException("OrderId is required."));
        cmd.Parameters.AddWithValue("itemId", (object?)detail.ItemId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("itemName", (object?)detail.ItemName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("quantity", detail.Quantity);
        cmd.Parameters.AddWithValue("unitPrice", detail.UnitPrice);
        cmd.Parameters.AddWithValue("variantId", detail.VariantId ?? throw new InvalidOperationException("VariantId is required."));
        cmd.Parameters.AddWithValue("size", (object?)detail.Size ?? DBNull.Value);
        cmd.Parameters.AddWithValue("color", (object?)detail.Color ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }
}
