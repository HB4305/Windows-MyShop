using MyShop.Models;
using MyShop.Services;
using MyShop.Utils;
using Npgsql;

namespace MyShop.Repositories;

public class SupplyRepository
{
    private readonly DbConnectionFactory _connFactory;

    public SupplyRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    public async Task<int> GetSuppliedProductCountByDateAsync(DateTime date)
    {
        var (start, end) = DateTimeUtils.GetDayRange(date);
        const string sql = @"
            SELECT COUNT(DISTINCT sd.item_id)
            FROM supplyorders so
            JOIN supplydetails sd ON sd.supply_id = so.id
            WHERE so.import_date >= @start
              AND so.import_date < @end
              AND sd.item_id IS NOT NULL";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> GetSuppliedProductCountByMonthAsync(DateTime referenceTime)
    {
        var prevMonth = referenceTime.AddMonths(-1);
        var start = new DateTime(prevMonth.Year, prevMonth.Month, 1);
        var end = start.AddMonths(1);

        const string sql = @"
            SELECT COUNT(DISTINCT sd.item_id)
            FROM supplyorders so
            JOIN supplydetails sd ON sd.supply_id = so.id
            WHERE so.import_date >= @start
              AND so.import_date < @end
              AND sd.item_id IS NOT NULL";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<(List<SupplyOrder> Items, int TotalCount)> GetOrdersAsync(int page, int pageSize)
    {
        var items = new List<SupplyOrder>();
        int totalCount = 0;
        var offset = (page - 1) * pageSize;

        var sql = @"
            SELECT COUNT(*) OVER() as total_count, 
                   so.id, so.supplier_id, s.name as supplier_name, so.import_date, so.total_cost
            FROM supplyorders so
            LEFT JOIN suppliers s ON so.supplier_id = s.id
            ORDER BY so.import_date DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("offset", offset);
        cmd.Parameters.AddWithValue("pageSize", pageSize);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(0);
            items.Add(new SupplyOrder
            {
                Id = reader.GetInt32(1),
                SupplierId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                SupplierName = reader.IsDBNull(3) ? null : reader.GetString(3),
                ImportDate = reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTime>(4),
                TotalCost = reader.IsDBNull(5) ? null : reader.GetDecimal(5)
            });
        }

        return (items, totalCount);
    }

    public async Task<int> CreateSupplyOrderTransactionAsync(SupplyOrder order, List<SupplyDetail> details)
    {
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            // 1. Insert SupplyOrder
            const string insertOrderSql = @"
                INSERT INTO supplyorders (supplier_id, import_date, total_cost)
                VALUES (@supplierId, @importDate, @totalCost)
                RETURNING id";

            await using var cmdOrder = new NpgsqlCommand(insertOrderSql, conn, tx);
            cmdOrder.Parameters.AddWithValue("supplierId", (object?)order.SupplierId ?? DBNull.Value);
            cmdOrder.Parameters.AddWithValue("importDate", order.ImportDate ?? DateTime.Now);
            cmdOrder.Parameters.AddWithValue("totalCost", order.TotalCost ?? 0m);

            var orderIdResult = await cmdOrder.ExecuteScalarAsync();
            int newOrderId = Convert.ToInt32(orderIdResult);
            order.Id = newOrderId;

            // 2. Insert SupplyDetails & Update Stock
            const string insertDetailSql = @"
                INSERT INTO supplydetails (supply_id, item_id, variant_id, quantity, import_price)
                VALUES (@supplyId, @itemId, @variantId, @quantity, @importPrice)";

            const string updateVariantStockSql = @"
                UPDATE sportitem_variants 
                SET stock_quantity = stock_quantity + @quantity 
                WHERE id = @variantId";

            const string updateItemStockFallbackSql = @"
                UPDATE sportitems 
                SET stock_quantity = stock_quantity + @quantity,
                    cost_price = @importPrice
                WHERE id = @itemId";

            const string updateCostPriceSql = @"
                UPDATE sportitems 
                SET cost_price = @importPrice 
                WHERE id = @itemId";

            foreach (var detail in details)
            {
                // Insert detail
                await using var cmdDetail = new NpgsqlCommand(insertDetailSql, conn, tx);
                cmdDetail.Parameters.AddWithValue("supplyId", newOrderId);
                cmdDetail.Parameters.AddWithValue("itemId", (object?)detail.ItemId ?? DBNull.Value);
                cmdDetail.Parameters.AddWithValue("variantId", (object?)detail.VariantId ?? DBNull.Value);
                cmdDetail.Parameters.AddWithValue("quantity", detail.Quantity);
                cmdDetail.Parameters.AddWithValue("importPrice", detail.ImportPrice);
                await cmdDetail.ExecuteNonQueryAsync();

                // Update stock in sportitem_variants or fallback to sportitems
                if (detail.VariantId.HasValue)
                {
                    await using var cmdStock = new NpgsqlCommand(updateVariantStockSql, conn, tx);
                    cmdStock.Parameters.AddWithValue("variantId", detail.VariantId.Value);
                    cmdStock.Parameters.AddWithValue("quantity", detail.Quantity);
                    await cmdStock.ExecuteNonQueryAsync();

                    // Still need to update cost_price on parent sportitem
                    if (detail.ItemId.HasValue)
                    {
                        await using var cmdCost = new NpgsqlCommand(updateCostPriceSql, conn, tx);
                        cmdCost.Parameters.AddWithValue("itemId", detail.ItemId.Value);
                        cmdCost.Parameters.AddWithValue("importPrice", detail.ImportPrice);
                        await cmdCost.ExecuteNonQueryAsync();
                    }
                }
                else if (detail.ItemId.HasValue)
                {
                    // Note: If triggers enforce stock from variants, this manual update 
                    // on sportitems might be overridden to 0 if no variant exists.
                    // Ideally, every sportitem has at least one variant.
                    await using var cmdStock = new NpgsqlCommand(updateItemStockFallbackSql, conn, tx);
                    cmdStock.Parameters.AddWithValue("itemId", detail.ItemId.Value);
                    cmdStock.Parameters.AddWithValue("quantity", detail.Quantity);
                    cmdStock.Parameters.AddWithValue("importPrice", detail.ImportPrice);
                    await cmdStock.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return newOrderId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
