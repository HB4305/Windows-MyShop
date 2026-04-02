using System;
using System.Collections.Generic;
using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Repositories;
using Supabase;

namespace MyShop.Services;

public class SportItemService
{
    private readonly SportItemRepository _repository;
    private readonly Supabase.Client _client;

    public SportItemService(SportItemRepository repository, Supabase.Client client)
    {
        _repository = repository;
        _client = client;
    }

    // Đếm tổng sản phẩm
    public Task<int> GetTotalCountAsync()
        => _repository.GetTotalCountAsync();

    public Task<List<DashboardLowStockProduct>> GetLowStockProductsAsync(int threshold = 5, int limit = 5)
            => _repository.GetLowStockProductsAsync(threshold, limit);

    public async Task<(List<SportItem> Items, int TotalCount)> GetItemsAsync(
        int page, int pageSize, string keyword, decimal? minPrice, decimal? maxPrice, string sortField, bool sortAscending)
    {
        return await _repository.GetItemsAsync(page, pageSize, keyword, minPrice, maxPrice, sortField, sortAscending);
    }

    public Task<List<string>> GetProductNamesAsync(int? categoryId = null)
        => _repository.GetProductNamesAsync(categoryId);

    public async Task<int> AddAsync(SportItem item) => await _repository.AddAsync(item);

    public async Task UpdateAsync(SportItem item) => await _repository.UpdateAsync(item);

    public async Task DeleteAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<List<ProductImage>> GetImagesAsync(int itemId) => await _repository.GetImagesAsync(itemId);

    public async Task AddImageAsync(ProductImage image) => await _repository.AddImageAsync(image);

    public async Task DeleteImageAsync(int imageId) => await _repository.DeleteImageAsync(imageId);

    public async Task<string> UploadImageAsync(byte[] bytes, string fileName)
    {
        var bucket = _client.Storage.From("sport image");
        var supabasePath = $"{Guid.NewGuid()}_{fileName}";
        await bucket.Upload(bytes, supabasePath, new Supabase.Storage.FileOptions { Upsert = true });
        return bucket.GetPublicUrl(supabasePath);
    }
}
