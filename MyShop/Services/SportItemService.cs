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
    private readonly string _imagesBasePath;

    public SportItemService(SportItemRepository repository)
    {
        _repository = repository;

        // Save image to the app data folder
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
        int? categoryId)
        => _repository.SearchForPosAsync(page, pageSize, keyword, categoryId);

    public Task<List<string>> GetProductNamesAsync(int? categoryId = null)
        => _repository.GetProductNamesAsync(categoryId);

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
    /// Saves an image to the app data folder and returns the file path (used as a URL).
    /// </summary>
    public async Task<string> UploadImageAsync(byte[] bytes, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var safeName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_imagesBasePath, safeName);
        await File.WriteAllBytesAsync(filePath, bytes);
        return filePath; // used as image URL
    }
}
