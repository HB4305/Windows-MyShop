using System;
using System.Collections.Generic;
using System.IO;
using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Repositories;

namespace MyShop.Services;

public class SportItemService
{
    private readonly SportItemRepository _repository;
    private readonly SupabaseStorageService _storageService;
    private readonly string _imagesBasePath;

    public SportItemService(SportItemRepository repository, SupabaseStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;

        // Save image to the app data folder (Keep for fallback/legacy)
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _imagesBasePath = Path.Combine(appData, "MyShop", "Images");
        Directory.CreateDirectory(_imagesBasePath);
    }

    public Task<int> GetTotalCountAsync()
        => _repository.GetTotalCountAsync();

    public Task<List<DashboardLowStockProduct>> GetLowStockProductsAsync(int threshold = 5, int limit = 5)
        => _repository.GetLowStockProductsAsync(threshold, limit);

    public Task<(List<SportItemListRow> Items, int TotalCount)> GetItemsAsync(
        int page, int pageSize, string keyword, decimal? minPrice, decimal? maxPrice, string sortField, bool sortAscending)
        => _repository.GetItemsAsync(page, pageSize, keyword, minPrice, maxPrice, sortField, sortAscending);

    public Task<PagedResult<SportItemListRow>> SearchForPosAsync(
        int page,
        int pageSize,
        string? keyword,
        string? categoryName)
        => _repository.SearchForPosAsync(page, pageSize, keyword, categoryName);

    public Task<List<string>> GetProductNamesAsync(int? categoryId = null)
        => _repository.GetProductNamesAsync(categoryId);

    public Task<List<string>> GetProductNamesByCategoryNameAsync(string? categoryName)
        => _repository.GetProductNamesByCategoryNameAsync(categoryName);

    public Task<int> AddAsync(SportItem item)
        => _repository.AddAsync(item);

    public Task UpdateAsync(SportItem item)
        => _repository.UpdateAsync(item);

    public Task DeleteAsync(int id)
        => _repository.DeleteAsync(id);

    public Task<List<ProductImage>> GetImagesAsync(int itemId)
        => _repository.GetImagesAsync(itemId);

    public Task AddImageAsync(ProductImage image)
        => _repository.AddImageAsync(image);

    public Task DeleteImageAsync(int imageId)
        => _repository.DeleteImageAsync(imageId);

    /// <summary>
    /// Resizes, compresses, and uploads an image to Supabase Storage.
    /// Returns the public URL.
    /// </summary>
    public async Task<string> UploadImageAsync(byte[] bytes, string fileName)
    {
        try
        {
            // 1. Optimize image (Resize & Compress)
            var optimizedBytes = Utils.ImageHelper.CompressAndResize(bytes);

            // 2. Upload to Supabase Storage
            var cloudUrl = await _storageService.UploadFileAsync(optimizedBytes, fileName);
            return cloudUrl;
        }
        catch (Exception)
        {
            // Fallback to local if Cloud fails (or just rethrow)
            var extension = Path.GetExtension(fileName);
            var safeName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_imagesBasePath, safeName);
            await File.WriteAllBytesAsync(filePath, bytes);
            return filePath;
        }
    }
}
