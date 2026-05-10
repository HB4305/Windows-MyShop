using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public partial class CustomerOrderViewModel : ObservableObject
{
    private readonly CustomerOrderService _service;
    private readonly CurrentUserService _currentUserService;
    private readonly SettingsManager _settingsManager;

    public CustomerOrderViewModel(CustomerOrderService orderService, CurrentUserService currentUserService, SettingsManager settingsManager)
    {
        _service = orderService;
        _currentUserService = currentUserService;
        _settingsManager = settingsManager;

        _ordersPerPage = _settingsManager.GetItemsPerPage();
    }

    // ── Collections ──────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<CustomerOrder> _orders = new();

    // ── UI State ──────────────────────────────────────────────────
    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _showDetailPanel = false;

    [ObservableProperty]
    private bool _isOwnerView = true; // Owner sees all; Sale only sees their own orders

    // ── Active Tab ──────────────────────────────────────────────────
    [ObservableProperty]
    private string _activeTab = "All";

    // ── Pagination ──────────────────────────────────────────────────
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _ordersPerPage = 10;

    [ObservableProperty]
    private ObservableCollection<CustomerOrder> _paginatedOrders = new();

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _startIndex = 0;

    [ObservableProperty]
    private int _endIndex = 0;

    [ObservableProperty]
    private int _filteredCount = 0;

    // ── Filter Options ──────────────────────────────────────────────
    public ObservableCollection<string> TabOptions { get; } = new()
    {
        "All",
        "Pending",
        "Processing",
        "Completed",
        "Delivered",
        "Cancelled",
        "Shipped"
    };

    // ── Computed Stats ─────────────────────────────────────────────
    [ObservableProperty]
    private int _totalOrders = 0;

    [ObservableProperty]
    private int _pendingCount = 0;

    [ObservableProperty]
    private int _processingCount = 0;

    [ObservableProperty]
    private int _completedCount = 0;

    [ObservableProperty]
    private int _cancelledCount = 0;

    [ObservableProperty]
    private decimal _totalRevenue = 0m;

    // ── Helpers ────────────────────────────────────────────────────
    private int GetTabCount(string tab)
        => tab == "All" ? TotalOrders
         : tab == "Pending" ? PendingCount
         : tab == "Processing" ? ProcessingCount
         : tab == "Completed" ? CompletedCount
         : tab == "Cancelled" ? CancelledCount
         : 0;

    // ── Load Orders ───────────────────────────────────────────────
    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        OrdersPerPage = _settingsManager.GetItemsPerPage();
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            List<CustomerOrder> orders;

            // Permissions: sale only sees their own orders
            if (_currentUserService.IsSale && _currentUserService.UserId.HasValue)
            {
                IsOwnerView = false;
                orders = await _service.GetOrdersBySellerAsync(_currentUserService.UserId.Value);
            }
            else
            {
                IsOwnerView = true;
                orders = await _service.GetAllOrdersAsync();
            }

            Orders = new ObservableCollection<CustomerOrder>(orders);
            RefreshStats();
            ApplyPagination();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void RefreshStats()
    {
        TotalOrders = Orders.Count;
        PendingCount = Orders.Count(o => o.Status == "Pending");
        ProcessingCount = Orders.Count(o => o.Status == "Processing");
        CompletedCount = Orders.Count(o => IsCompletedStatus(o.Status));
        CancelledCount = Orders.Count(o => o.Status == "Cancelled");
        TotalRevenue = Orders
            .Where(o => o.PaymentStatus == "Paid")
            .Sum(o => o.TotalAmount ?? 0);
    }

    // ── Tab Selection ─────────────────────────────────────────────
    partial void OnActiveTabChanged(string value)
    {
        CurrentPage = 1;
        ApplyPagination();
    }

    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        ApplyPagination();
    }

    // ── Pagination ─────────────────────────────────────────────────
    [RelayCommand]
    public void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        ApplyPagination();
    }

    [RelayCommand]
    public void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            ApplyPagination();
        }
    }

    [RelayCommand]
    public void PrevPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            ApplyPagination();
        }
    }

    private void ApplyPagination()
    {
        var filteredList = GetFilteredOrders().OrderByDescending(o => o.CreatedAt).ToList();
        FilteredCount = filteredList.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)FilteredCount / OrdersPerPage));
        CurrentPage = Math.Min(CurrentPage, TotalPages);

        PaginatedOrders = new ObservableCollection<CustomerOrder>(
            filteredList.Skip((CurrentPage - 1) * OrdersPerPage).Take(OrdersPerPage));

        StartIndex = FilteredCount > 0 ? (CurrentPage - 1) * OrdersPerPage + 1 : 0;
        EndIndex = Math.Min(CurrentPage * OrdersPerPage, FilteredCount);
    }

    private IEnumerable<CustomerOrder> GetFilteredOrders()
    {
        var query = SearchQuery?.Trim().ToLower() ?? "";
        var source = Orders.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
        {
            source = source.Where(o =>
                (o.CustomerName?.ToLower().Contains(query) ?? false) ||
                (o.CustomerPhone?.ToLower().Contains(query) ?? false) ||
                (o.Id.ToString().Contains(query)));
        }

        if (ActiveTab != "All")
        {
            var tabStatus = ActiveTab switch
            {
                "Completed" => null,
                _ => ActiveTab
            };
            source = tabStatus is null
                ? source.Where(o => IsCompletedStatus(o.Status))
                : source.Where(o => o.Status == tabStatus);
        }

        return source;
    }

    private static bool IsCompletedStatus(string? status)
        => status is "Completed" or "Delivered";

    // ── Select Order → Detail Panel ────────────────────────────────
    public async Task SelectOrderAsync(CustomerOrder order)
    {
        if (order == null) return;

        // Get from the already loaded list (no DB query)
        SelectedOrder = Orders.FirstOrDefault(o => o.Id == order.Id) ?? order;
        SelectedOrderId = SelectedOrder.Id;
        ShowDetailPanel = true;

        // Only load details when needed (lazy load)
        var details = await _service.GetOrderDetailsAsync(order.Id);
        CurrentDetails = new ObservableCollection<OrderDetail>(details);
    }

    // ── Close Detail Panel ───────────────────────────────────────
    [RelayCommand]
    public void CloseDetailPanel()
    {
        ShowDetailPanel = false;
        SelectedOrderId = 0;
        SelectedOrder = new();
        CurrentDetails.Clear();
    }

    // ── Update Order Status ──────────────────────────────────────
    [RelayCommand]
    public async Task UpdateOrderStatusAsync(string status)
    {
        if (SelectedOrder?.Id == 0) return;
        ErrorMessage = null;
        try
        {
            await _service.UpdateStatusAsync(SelectedOrder!.Id, status);
            SelectedOrder.Status = status;
            // Update the corresponding row in the list instead of reloading everything
            var existing = Orders.FirstOrDefault(o => o.Id == SelectedOrder.Id);
            if (existing != null) existing.Status = status;

            RefreshStats();
            ApplyPagination();
            OnPropertyChanged(nameof(SelectedOrder));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    // ── Update Payment Status ────────────────────────────────────
    [RelayCommand]
    public async Task UpdatePaymentStatusAsync(string paymentStatus)
    {
        if (SelectedOrder?.Id == 0) return;
        ErrorMessage = null;
        try
        {
            await _service.UpdatePaymentStatusAsync(SelectedOrder!.Id, paymentStatus);
            SelectedOrder.PaymentStatus = paymentStatus;
            var existing = Orders.FirstOrDefault(o => o.Id == SelectedOrder.Id);
            if (existing != null) existing.PaymentStatus = paymentStatus;

            RefreshStats();
            ApplyPagination();
            OnPropertyChanged(nameof(SelectedOrder));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    // ── Delete Order ─────────────────────────────────────────────
    [RelayCommand]
    public async Task DeleteOrderAsync()
    {
        if (SelectedOrder?.Id == null) return;
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await _service.DeleteOrderAsync(SelectedOrder.Id);
            CloseDetailPanel();
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Properties for Detail Panel ─────────────────────────────
    // ── Selected Order ID (for row highlight) ──────────────────────
    [ObservableProperty]
    private int _selectedOrderId = 0;

    [ObservableProperty]
    private CustomerOrder _selectedOrder = new();

    [ObservableProperty]
    private ObservableCollection<OrderDetail> _currentDetails = new();

    /// <summary>Alias for CurrentDetails — used by XAML binding.</summary>
    public ObservableCollection<OrderDetail> SelectedOrderDetails => CurrentDetails;
}
