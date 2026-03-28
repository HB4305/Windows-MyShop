using MyShop.Models;
using Postgrest;
using Supabase;

namespace MyShop.Repositories;

public class SportItemRepository
{
    private readonly Supabase.Client _client;

    public SportItemRepository(Supabase.Client client) => _client = client;

    public async Task<(List<SportItem> Items, int TotalCount)> GetItemsAsync(
        int page, int pageSize, string keyword, decimal? minPrice, decimal? maxPrice, string sortField, bool sortAscending)
    {
        var query = (Postgrest.Table<SportItem>)_client.From<SportItem>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Filter("name", Constants.Operator.ILike, $"%{keyword}%");
        }
        if (minPrice.HasValue)
        {
            query = query.Where(x => x.SellingPrice >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(x => x.SellingPrice <= maxPrice.Value);
        }

        if (!string.IsNullOrEmpty(sortField))
        {
            var ordering = sortAscending ? Constants.Ordering.Ascending : Constants.Ordering.Descending;
            query = query.Order(sortField, ordering);
        }

        int from = (page - 1) * pageSize;
        int to = from + pageSize - 1;

        // Try getting exact count. Postgrest has Count Option
        var response = await query.Range(from, to).Get();
        
        // Cần đếm tổng số thực sự vì dùng UI Phân trang
        // Để query lại 1 lần nữa để lấy count 
        var countQuery = (Postgrest.Table<SportItem>)_client.From<SportItem>();
        if (!string.IsNullOrWhiteSpace(keyword)) countQuery = countQuery.Filter("name", Constants.Operator.ILike, $"%{keyword}%");
        if (minPrice.HasValue) countQuery = countQuery.Where(x => x.SellingPrice >= minPrice.Value);
        if (maxPrice.HasValue) countQuery = countQuery.Where(x => x.SellingPrice <= maxPrice.Value);
        
        var countResponse = await countQuery.Count(Constants.CountType.Exact);
        int totalCount = countResponse; // in Supabase-csharp, Count method returns Task<int> if exact count

        return (response.Models ?? new List<SportItem>(), totalCount);
    }

    public async Task<int> AddAsync(SportItem item)
    {
        var result = await _client.From<SportItem>().Insert(item);
        return result.Models.FirstOrDefault()?.Id ?? 0;
    }

    public async Task UpdateAsync(SportItem item)
        => await _client.From<SportItem>().Update(item);

    public async Task DeleteAsync(int id)
        => await _client.From<SportItem>().Where(x => x.Id == id).Delete();

    public async Task<List<ProductImage>> GetImagesAsync(int itemId)
    {
        var response = await _client.From<ProductImage>()
            .Where(x => x.ItemId == itemId)
            .Get();
        return response.Models ?? new List<ProductImage>();
    }

    public async Task AddImageAsync(ProductImage image)
        => await _client.From<ProductImage>().Insert(image);
        
    public async Task DeleteImageAsync(int imageId)
        => await _client.From<ProductImage>().Where(x => x.Id == imageId).Delete();
}
