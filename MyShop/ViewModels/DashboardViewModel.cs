using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MyShop.Models;
using MyShop.Models.DashboardModels;
using MyShop.Services;
using MyShop.Views.Dialogs;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly SportItemService _sportItemService;
    private readonly OrderService _orderService;
    private readonly CustomerOrderService _customerOrderService;
    private readonly CurrentUserService _currentUserService;

    public DashboardViewModel(
        SportItemService sportItemService,
        OrderService orderService,
        CustomerOrderService customerOrderService,
        CurrentUserService currentUserService)
    {
        _sportItemService = sportItemService;
        _orderService = orderService;
        _customerOrderService = customerOrderService;
        _currentUserService = currentUserService;
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
    public async Task CreateOrderAsync()
    {
        Console.WriteLine("[DashboardVM] CreateOrder command triggered.");
        try
        {
            var dialog = new CreateOrderDialog();
            
            // In WinUI 3 Desktop, we MUST set XamlRoot for ContentDialog
            var mainWindow = ((App)App.Current).MainWindow;
            if (mainWindow?.Content?.XamlRoot != null)
            {
                dialog.XamlRoot = mainWindow.Content.XamlRoot;
            }
            else
            {
                Console.Error.WriteLine("[DashboardVM] Error: XamlRoot is null, cannot show dialog.");
                ErrorMessage = "Cannot open dialog: UI root not ready.";
                return;
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var order = dialog.ViewModel.Order;
                var details = dialog.ViewModel.OrderDetails.ToList();
                // Assign current user's seller_id and seller_name to the new order
                var sellerId = _currentUserService.UserId ?? 0;
                var sellerName = _currentUserService.UserEmail ?? "";
                await _customerOrderService.CreateOrderAsync(order, details, sellerId, sellerName);
                
                // Refresh dashboard stats
                await LoadDashboardAsync();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[DashboardVM] CreateOrder ERROR: {ex}");
            ErrorMessage = $"Failed to create order: {ex.Message}";
        }
    }

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
            var rawPoints = dailyRevenuePointsTask.Result;
            var pointsMap = new Dictionary<DateTime, RevenueReport>();
            foreach (var p in rawPoints)
            {
                pointsMap[p.Date.Date] = p;
            }

            var start = new DateTime(now.Year, now.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var monthPoints = new List<RevenueReport>(daysInMonth);
            for (int i = 0; i < daysInMonth; i++)
            {
                var day = start.AddDays(i);
                if (pointsMap.TryGetValue(day, out var dayReport))
                {
                    monthPoints.Add(dayReport);
                }
                else
                {
                    monthPoints.Add(new RevenueReport { Date = day, TotalOrders = 0, GrossRevenue = 0m });
                }
            }
            DailyRevenuePoints = monthPoints;

            // Re-evaluate computed properties
            OnPropertyChanged(nameof(TotalProductsDisplay));
            OnPropertyChanged(nameof(TotalTodayOrdersDisplay));
            OnPropertyChanged(nameof(TodayRevenueDisplay));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[DashboardVM] LoadDashboardAsync ERROR: {e}");
            ErrorMessage = $"Error: {e.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
