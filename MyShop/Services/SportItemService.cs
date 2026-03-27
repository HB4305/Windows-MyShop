using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Repositories;

namespace MyShop.Services;

public class SportItemService
{
	private readonly SportItemRepository _repository;

	public SportItemService(SportItemRepository repository) => _repository = repository;

	// Đếm tổng sản phẩm
	public Task<int> GetTotalCountAsync()
		=> _repository.GetTotalCountAsync();

	public Task<List<DashboardLowStockProduct>> GetLowStockProductsAsync(int threshold = 5, int limit = 5)
			=> _repository.GetLowStockProductsAsync(threshold, limit);
}
