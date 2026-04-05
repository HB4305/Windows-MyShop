using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Services;
using Npgsql;

namespace MyShop.Repositories;

public class SportItemRepository
{
    private readonly DbConnectionFactory _connFactory;

    public SportItemRepository(DbConnectionFactory connFactory) => _connFactory = connFactory;

    public async Task<int> GetTotalCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM sportitems";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<List<DashboardLowStockProduct>> GetLowStockProductsAsync(int threshold = 5, int limit = 5)
    {
        const string sql = @"
            SELECT id, name, stock_quantity, image_urls
            FROM sportitems
            WHERE stock_quantity < @threshold
            ORDER BY stock_quantity ASC
            LIMIT @limit";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("threshold", threshold);
        cmd.Parameters.AddWithValue("limit", limit);

        var results = new List<DashboardLowStockProduct>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var urls = reader.GetFieldValue<string[]>(3);
            results.Add(new DashboardLowStockProduct
            {
                ItemId = reader.GetInt32(0),
                Name = reader.GetString(1),
                StockQuantity = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                ImageUrls = urls ?? Array.Empty<string>()
            });
        }
        return results;
    }

    public async Task<(List<SportItem> Items, int TotalCount)> GetItemsAsync(
        int page, int pageSize, string keyword, decimal? minPrice, decimal? maxPrice, string sortField, bool sortAscending)
    {
        var items = new List<SportItem>();
        int totalCount;

        // Build ORDER BY safely (only allow known columns)
        var allowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "id", "name", "selling_price", "stock_quantity", "category_id", "created_at" };
        var safeSort = allowedSortFields.Contains(sortField) ? sortField : "id";
        var order = sortAscending ? "ASC" : "DESC";
        var offset = (page - 1) * pageSize;

        // Build WHERE clause
        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
            conditions.Add("name ILIKE @keyword");
        if (minPrice.HasValue)
            conditions.Add("selling_price >= @minPrice");
        if (maxPrice.HasValue)
            conditions.Add("selling_price <= @maxPrice");
        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        const string countSql = "SELECT COUNT(*) FROM sportitems";
        var dataSql = $@"
             SELECT id, category_id, name,
                 cost_price, selling_price, stock_quantity,
                   low_stock_threshold, image_urls
            FROM sportitems
            {where}
            ORDER BY {safeSort} {order}
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();

        // Count
        await using (var cmd = new NpgsqlCommand(countSql, conn))
        {
            totalCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // Data
        await using (var cmd = new NpgsqlCommand(dataSql, conn))
        {
            if (!string.IsNullOrWhiteSpace(keyword))
                cmd.Parameters.AddWithValue("keyword", $"%{keyword}%");
            if (minPrice.HasValue)
                cmd.Parameters.AddWithValue("minPrice", minPrice.Value);
            if (maxPrice.HasValue)
                cmd.Parameters.AddWithValue("maxPrice", maxPrice.Value);
            cmd.Parameters.AddWithValue("offset", offset);
            cmd.Parameters.AddWithValue("pageSize", pageSize);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(ReadSportItem(reader));
            }
        }

        if (items.Count > 0)
        {
            await LoadVariantsForItemsAsync(conn, items);
        }

        return (items, totalCount);
    }

    public async Task<List<string>> GetProductNamesAsync(int? categoryId = null)
    {
        var sql = string.IsNullOrEmpty(categoryId?.ToString())
            ? "SELECT DISTINCT name FROM sportitems WHERE name IS NOT NULL AND name != '' ORDER BY name"
            : "SELECT DISTINCT name FROM sportitems WHERE name IS NOT NULL AND name != '' AND category_id = @categoryId ORDER BY name";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        if (categoryId.HasValue)
            cmd.Parameters.AddWithValue("categoryId", categoryId.Value);

        var names = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            names.Add(reader.GetString(0));
        return names;
    }

    public async Task<int> AddAsync(SportItem item)
    {
        const string sql = @"
            INSERT INTO sportitems (category_id, name,
                                   cost_price, selling_price, stock_quantity,
                                   low_stock_threshold, image_urls)
            VALUES (@categoryId, @name,
                    @costPrice, @sellingPrice, @stockQuantity,
                    @lowStockThreshold, @imageUrls)
            RETURNING id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        AddSportItemParams(cmd, item);

        var result = await cmd.ExecuteScalarAsync();
        var newId = Convert.ToInt32(result);
        await ReplaceVariantsAsync(conn, newId, item.Variants);
        return newId;
    }

    public async Task UpdateAsync(SportItem item)
    {
        const string sql = @"
            UPDATE sportitems SET
                category_id = @categoryId, name = @name, cost_price = @costPrice,
                selling_price = @sellingPrice, stock_quantity = @stockQuantity,
                low_stock_threshold = @lowStockThreshold, image_urls = @imageUrls
            WHERE id = @id";

        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", item.Id);
        AddSportItemParams(cmd, item);
        await cmd.ExecuteNonQueryAsync();
        await ReplaceVariantsAsync(conn, item.Id, item.Variants);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM sportitems WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<ProductImage>> GetImagesAsync(int itemId)
    {
        const string sql = "SELECT id, item_id, image_url FROM product_images WHERE item_id = @itemId";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("itemId", itemId);

        var images = new List<ProductImage>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            images.Add(new ProductImage
            {
                Id = reader.GetInt32(0),
                ItemId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                ImageUrl = reader.GetString(2)
            });
        }
        return images;
    }

    public async Task AddImageAsync(ProductImage image)
    {
        const string sql = "INSERT INTO product_images (item_id, image_url) VALUES (@itemId, @imageUrl)";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("itemId", (object?)image.ItemId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("imageUrl", image.ImageUrl);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteImageAsync(int imageId)
    {
        const string sql = "DELETE FROM product_images WHERE id = @id";
        await using var conn = _connFactory.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", imageId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static void AddSportItemParams(NpgsqlCommand cmd, SportItem item)
    {
        cmd.Parameters.AddWithValue("categoryId", item.CategoryId);
        cmd.Parameters.AddWithValue("name", item.Name);
        cmd.Parameters.AddWithValue("costPrice", (object?)item.CostPrice ?? DBNull.Value);
        cmd.Parameters.AddWithValue("sellingPrice", (object?)item.SellingPrice ?? DBNull.Value);
        cmd.Parameters.AddWithValue("stockQuantity", item.StockQuantity ?? 0);
        cmd.Parameters.AddWithValue("lowStockThreshold", (object?)item.LowStockThreshold ?? DBNull.Value);
        cmd.Parameters.AddWithValue("imageUrls", item.ImageUrls.ToArray());
    }

    private static async Task ReplaceVariantsAsync(NpgsqlConnection conn, int itemId, List<SportItemVariant> variants)
    {
        await using (var del = new NpgsqlCommand("DELETE FROM sportitem_variants WHERE sportitem_id = @itemId", conn))
        {
            del.Parameters.AddWithValue("itemId", itemId);
            await del.ExecuteNonQueryAsync();
        }

        if (variants.Count == 0)
            return;

        const string insertSql = @"
            INSERT INTO sportitem_variants (sportitem_id, size, color, stock_quantity, sku)
            VALUES (@itemId, @size, @color, @stockQuantity, @sku)";

        foreach (var v in variants)
        {
            await using var cmd = new NpgsqlCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("itemId", itemId);
            cmd.Parameters.AddWithValue("size", (object?)v.Size ?? DBNull.Value);
            cmd.Parameters.AddWithValue("color", (object?)v.Color ?? DBNull.Value);
            cmd.Parameters.AddWithValue("stockQuantity", Math.Max(0, v.StockQuantity));
            cmd.Parameters.AddWithValue("sku", (object?)v.Sku ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task LoadVariantsForItemsAsync(NpgsqlConnection conn, List<SportItem> items)
    {
        var itemById = items.ToDictionary(i => i.Id);
        var itemIds = itemById.Keys.ToArray();

        const string sql = @"
            SELECT id, sportitem_id, size, color, stock_quantity, sku
            FROM sportitem_variants
            WHERE sportitem_id = ANY(@itemIds)
            ORDER BY id";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("itemIds", itemIds);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var itemId = reader.GetInt32(1);
            if (!itemById.TryGetValue(itemId, out var item))
                continue;

            item.Variants.Add(new SportItemVariant
            {
                Id = reader.GetInt32(0),
                SportItemId = itemId,
                Size = reader.IsDBNull(2) ? null : reader.GetString(2),
                Color = reader.IsDBNull(3) ? null : reader.GetString(3),
                StockQuantity = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                Sku = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }
    }

    private static SportItem ReadSportItem(NpgsqlDataReader reader)
    {
        return new SportItem
        {
            Id = reader.GetInt32(0),
            CategoryId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
            Name = reader.IsDBNull(2) ? "" : reader.GetString(2),
            CostPrice = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
            SellingPrice = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
            StockQuantity = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
            LowStockThreshold = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            ImageUrls = reader.IsDBNull(7) ? new List<string>() : reader.GetFieldValue<string[]>(7).ToList()
        };
    }
}
