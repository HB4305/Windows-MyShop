using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
  private readonly SportItemService _sportItemService;
  private readonly OrderService _orderService;
  private readonly SupplyService _supplyService;

  public DashboardViewModel(
    SportItemService sportItemService,
    OrderService orderService,
    SupplyService supplyService
  ) {
    _sportItemService = sportItemService;
    _orderService = orderService;
    _supplyService = supplyService;
    _ = LoadDashboardAsync();
  }

  // For UI indications
  [ObservableProperty]
  private bool _isLoading;

  [ObservableProperty]
  private string _errorMessage = string.Empty;

  [ObservableProperty]
  private string _output = "Dang tai dashboard...";

  [ObservableProperty]
  private string _topSellerSummary = "Dang tai top seller...";

  // Attributes

  // Tổng sản phẩm
  [ObservableProperty]
  private int _totalProducts = 0;
  // Số sản phẩm nhập thêm hôm nay
  [ObservableProperty]
  private int _todaySuppliedProducts = 0;
  // Tổng đơn hàng hôm nay
  [ObservableProperty]
  private int _totalTodayOrders = 0;
  // Trung bình số đơn hàng 7 ngày gần nhất
  [ObservableProperty]
  private int _avgWeeklyOrders = 0;
  // Tổng doanh thu trong ngày
  [ObservableProperty]
  private decimal _todayRevenue = 0;
  // Doanh thu hôm qua để so sánh
  [ObservableProperty]
  private decimal _PrevDayRevenue = 0;
  // Top 5 sản phẩm sắp hết hàng (số lượng < 5)
  [ObservableProperty]
  private List<DashboardLowStockProduct> _lowStockItems = [];
  // Top 5 sản phẩm bán chạy 
  [ObservableProperty]
  private List<DashboardTopSellerProduct> _topSellerItems = [];
  // Chi tiết 3 đơn hàng gần nhất
  [ObservableProperty]
  private List<DashboardRecentOrder> _recentOrders = [];
  // Biểu đồ doanh thu theo ngày trong tháng hiện tại
  [ObservableProperty]
  private List<RevenueReport> _dailyRevenuePoints = [];

  [RelayCommand]
  public async Task LoadDashboardAsync()
  {
    try
    {
      IsLoading = true;
      ErrorMessage = string.Empty;
      Output = "Dang tai dashboard...";
      TopSellerSummary = "Dang tai top seller...";

      var now = DateTime.Now;

      var totalProductsTask = _sportItemService.GetTotalCountAsync();
      var todaySuppliedProductsTask = _supplyService.GetSuppliedProductCountByDateAsync(now);

      var totalTodayOrdersTask = _orderService.GetOrderCountByDateAsync(now);
      var avgWeeklyOrdersTask = _orderService.GetAvgWeeklyOrdersAsync(now);

      var todayRevenueTask = _orderService.GetRevenueByDateAsync(now);
      var prevDayRevenueTask = _orderService.GetPrevDateRevenueAsync(now);

      var topSellerItemsTask = _orderService.GetTopSellingProductsAsync(nDays: 30);
      var lowStockItemsTask = _sportItemService.GetLowStockProductsAsync();

      var recentOrdersTask = _orderService.GetRecentOrdersAsync();
      var dailyRevenuePointsTask = _orderService.GetMonthlyRevenueAsync(now);

      await Task.WhenAll(
        totalProductsTask,
        todaySuppliedProductsTask,
        lowStockItemsTask,
        topSellerItemsTask,
        totalTodayOrdersTask,
        avgWeeklyOrdersTask,
        todayRevenueTask,
        prevDayRevenueTask,
        recentOrdersTask,
        dailyRevenuePointsTask);

      TotalProducts = totalProductsTask.Result;
      TodaySuppliedProducts = todaySuppliedProductsTask.Result;
      TotalTodayOrders = totalTodayOrdersTask.Result;
      AvgWeeklyOrders = avgWeeklyOrdersTask.Result;
      TodayRevenue = todayRevenueTask.Result;
      PrevDayRevenue = prevDayRevenueTask.Result;
      LowStockItems = lowStockItemsTask.Result;
      TopSellerItems = topSellerItemsTask.Result;
      RecentOrders = recentOrdersTask.Result;
      DailyRevenuePoints = dailyRevenuePointsTask.Result;

      var lines = new List<string>
      {
        $"Total products: {TotalProducts}",
        $"Today supplied products: {TodaySuppliedProducts}",
        $"Today orders: {TotalTodayOrders}",
        $"Average weekly orders: {AvgWeeklyOrders}",
        $"Today revenue: {TodayRevenue:N2}",
        $"Previous day revenue: {PrevDayRevenue:N2}",
        "",
        "Top 5 low stock:"
      };

      if (LowStockItems.Count == 0)
      {
        lines.Add("- No data");
      }
      else
      {
        lines.AddRange(LowStockItems.Select(item =>
            $"- {item.Name} | Stock: {item.StockQuantity} | Images: {item.ImageUrlsText}"));
      }

      lines.Add("");
      lines.Add("Top 5 best sellers:");

      if (TopSellerItems.Count == 0)
      {
        lines.Add("- No data");
        TopSellerSummary = "Top seller: No data";
      }
      else
      {
        lines.AddRange(TopSellerItems.Select(item =>
            $"- {item.Name} | Sold: {item.QuantitySold} | Revenue: {item.CurrPeriodRevenue:N2} | Prev: {item.PrevPeriodRevenue:N2} | Images: {item.ImageUrlsText}"));

        TopSellerSummary = string.Join(
          Environment.NewLine,
          TopSellerItems.Select(item =>
            $"{item.Name} | Current: {item.CurrPeriodRevenue:N2} | Prev: {item.PrevPeriodRevenue:N2}")
        );
      }

      lines.Add("");
      lines.Add("Recent orders:");

      if (RecentOrders.Count == 0)
      {
        lines.Add("- No data");
      }
      else
      {
        lines.AddRange(RecentOrders.Select(order =>
            $"- #{order.Id} | {order.CustomerName} | {order.TotalPrice:N2} | {order.Status}"));
      }

      lines.Add("");
      lines.Add("Monthly revenue:");

      if (DailyRevenuePoints.Count == 0)
      {
        lines.Add("- No data");
      }
      else
      {
        lines.AddRange(DailyRevenuePoints.Select(point =>
            $"- {point.Date:yyyy-MM-dd} | Orders: {point.TotalOrders} | Revenue: {point.GrossRevenue:N2}"));
      }

      Output = string.Join(Environment.NewLine, lines);
    }
    catch (Exception e)
    {
      ErrorMessage = $"Loi: {e.Message}";
      Output = "Khong tai duoc du lieu dashboard.";
      TopSellerSummary = "Khong tai duoc top seller.";
    }
    finally
    {
      IsLoading = false;
    }
  }
}
