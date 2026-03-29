using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    private readonly SupplyService _supplyService;

    public DashboardViewModel(
        SportItemService sportItemService,
        OrderService orderService,
        SupplyService supplyService)
    {
        _sportItemService = sportItemService;
        _orderService = orderService;
        _supplyService = supplyService;
        _ = LoadDashboardAsync();
    }

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // ── Raw data ────────────────────────────────────────────────────────
    [ObservableProperty] private int _totalProducts;
    [ObservableProperty] private int _todaySuppliedProducts;
    [ObservableProperty] private int _totalTodayOrders;
    [ObservableProperty] private int _avgWeeklyOrders;
    [ObservableProperty] private decimal _todayRevenue;
    [ObservableProperty] private decimal _prevDayRevenue;
    [ObservableProperty] private List<DashboardLowStockProduct> _lowStockItems = [];
    [ObservableProperty] private List<DashboardTopSellerProduct> _topSellerItems = [];
    [ObservableProperty] private List<DashboardRecentOrder> _recentOrders = [];
    [ObservableProperty] private List<RevenueReport> _dailyRevenuePoints = [];

    // ── Computed display properties (used by DashboardPage.xaml) ───────

    public string TotalProductsDisplay => TotalProducts.ToString("N0");

    public string TotalProductsTrend
    {
        get
        {
            if (TotalProducts == 0) return "No data";
            var pct = ((double)TodaySuppliedProducts / Math.Max(TotalProducts, 1)) * 100;
            return $"{pct:F1}% added this month";
        }
    }

    public string TotalTodayOrdersDisplay => TotalTodayOrders.ToString("N0");

    public string TodayOrdersTrend
    {
        get
        {
            if (AvgWeeklyOrders == 0) return "No avg data";
            var diff = TotalTodayOrders - AvgWeeklyOrders;
            var sign = diff >= 0 ? "+" : "";
            return $"{sign}{diff} vs avg daily";
        }
    }

    public string TodayRevenueDisplay => $"${TodayRevenue:N2}";

    public string TodayRevenueTrend
    {
        get
        {
            if (PrevDayRevenue == 0) return "No prev data";
            var diff = TodayRevenue - PrevDayRevenue;
            var pct = (double)(diff / Math.Max(PrevDayRevenue, 1)) * 100;
            var sign = pct >= 0 ? "+" : "";
            return $"{sign}{pct:F1}% vs yesterday";
        }
    }

    /// Returns "↑" (up) or "↓" (down) for revenue trend icon
    public string RevenueTrendIcon => TodayRevenue >= PrevDayRevenue ? "↑" : "↓";

    /// Returns Green or Red brush for revenue trend
    public SolidColorBrush RevenueTrendBrush =>
        TodayRevenue >= PrevDayRevenue
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 5, 150, 105))   // #059669 green
            : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 38, 38)); // #DC2626 red

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
                totalProductsTask, todaySuppliedProductsTask,
                lowStockItemsTask, topSellerItemsTask,
                totalTodayOrdersTask, avgWeeklyOrdersTask,
                todayRevenueTask, prevDayRevenueTask,
                recentOrdersTask, dailyRevenuePointsTask);

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

            // Re-evaluate computed properties
            OnPropertyChanged(nameof(TotalProductsDisplay));
            OnPropertyChanged(nameof(TotalProductsTrend));
            OnPropertyChanged(nameof(TotalTodayOrdersDisplay));
            OnPropertyChanged(nameof(TodayOrdersTrend));
            OnPropertyChanged(nameof(TodayRevenueDisplay));
            OnPropertyChanged(nameof(TodayRevenueTrend));
            OnPropertyChanged(nameof(RevenueTrendIcon));
            OnPropertyChanged(nameof(RevenueTrendBrush));
        }
        catch (Exception e)
        {
            ErrorMessage = $"Loi: {e.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
