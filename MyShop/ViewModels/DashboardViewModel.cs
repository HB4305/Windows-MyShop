using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly SportItemService _sportItemService;
    private readonly OrderService _orderService;

    public DashboardViewModel(
        SportItemService sportItemService,
        OrderService orderService)
    {
        _sportItemService = sportItemService;
        _orderService = orderService;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        Console.Error.WriteLine("[DashboardVM] Created, starting LoadDashboardAsync...");
        _ = LoadDashboardAsync();
    }

    // Catch all unobserved task exceptions for debugging
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Console.Error.WriteLine($"[DashboardVM] Unobserved exception: {e.Exception}");
        e.SetObserved();
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // ── Raw data ────────────────────────────────────────────────────────
    [ObservableProperty] private int _totalProducts;
    [ObservableProperty] private int _totalTodayOrders;
    [ObservableProperty] private decimal _todayRevenue;
    [ObservableProperty] private List<DashboardLowStockProduct> _lowStockItems = [];
    [ObservableProperty] private List<DashboardTopSellerProduct> _topSellerItems = [];
    [ObservableProperty] private List<DashboardRecentOrder> _recentOrders = [];
    [ObservableProperty] private List<RevenueReport> _dailyRevenuePoints = [];

    // ── Computed display properties (used by DashboardPage.xaml) ───────

    public string TotalProductsDisplay => TotalProducts.ToString("N0");
    public string TotalTodayOrdersDisplay => TotalTodayOrders.ToString("N0");
    public string TodayRevenueDisplay => $"${TodayRevenue:N2}";

    // ── Commands ─────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var now = DateTime.Now;

            var totalProductsTask = _sportItemService.GetTotalCountAsync();
            var totalTodayOrdersTask = _orderService.GetOrderCountByDateAsync(now);
            var todayRevenueTask = _orderService.GetRevenueByDateAsync(now);
            var topSellerItemsTask = _orderService.GetTopSellingProductsAsync(nDays: 30);
            var lowStockItemsTask = _sportItemService.GetLowStockProductsAsync();
            var recentOrdersTask = _orderService.GetRecentOrdersAsync();
            var dailyRevenuePointsTask = _orderService.GetMonthlyRevenueAsync(now);

            await Task.WhenAll(
                totalProductsTask,
                totalTodayOrdersTask,
                todayRevenueTask,
                lowStockItemsTask,
                topSellerItemsTask,
                recentOrdersTask,
                dailyRevenuePointsTask);

            TotalProducts = totalProductsTask.Result;
            TotalTodayOrders = totalTodayOrdersTask.Result;
            TodayRevenue = todayRevenueTask.Result;
            LowStockItems = lowStockItemsTask.Result;
            TopSellerItems = topSellerItemsTask.Result;
            RecentOrders = recentOrdersTask.Result;
            DailyRevenuePoints = dailyRevenuePointsTask.Result;

            // Re-evaluate computed properties
            OnPropertyChanged(nameof(TotalProductsDisplay));
            OnPropertyChanged(nameof(TotalTodayOrdersDisplay));
            OnPropertyChanged(nameof(TodayRevenueDisplay));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[DashboardVM] LoadDashboardAsync ERROR: {e}");
            ErrorMessage = $"Loi: {e.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
