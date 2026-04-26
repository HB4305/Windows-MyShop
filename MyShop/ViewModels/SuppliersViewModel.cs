using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.ViewModels;

public partial class SuppliersViewModel : ObservableObject
{
    private readonly SupplierRepository _supplierRepo;
    private readonly SupplyRepository _supplyRepo;

    public SuppliersViewModel(SupplierRepository supplierRepo, SupplyRepository supplyRepo)
    {
        _supplierRepo = supplierRepo;
        _supplyRepo = supplyRepo;
    }

    [ObservableProperty]
    private bool _isLoadingSuppliers;

    [ObservableProperty]
    private string _suppliersErrorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Supplier> _suppliers = [];

    [ObservableProperty]
    private string _searchSupplierKeyword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SupplierTotalPages))]
    [NotifyPropertyChangedFor(nameof(SupplierDisplayFrom))]
    [NotifyPropertyChangedFor(nameof(SupplierDisplayTo))]
    private int _supplierCurrentPage = 1;

    [ObservableProperty]
    private int _supplierPageSize = 20;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SupplierTotalPages))]
    [NotifyPropertyChangedFor(nameof(SupplierDisplayFrom))]
    [NotifyPropertyChangedFor(nameof(SupplierDisplayTo))]
    private int _supplierTotalItems;

    public int SupplierTotalPages => (SupplierTotalItems + SupplierPageSize - 1) / Math.Max(1, SupplierPageSize);
    public int SupplierDisplayFrom => SupplierTotalItems == 0 ? 0 : (SupplierCurrentPage - 1) * SupplierPageSize + 1;
    public int SupplierDisplayTo => Math.Min(SupplierCurrentPage * SupplierPageSize, SupplierTotalItems);

    [RelayCommand]
    public async Task LoadSuppliersAsync()
    {
        try
        {
            IsLoadingSuppliers = true;
            SuppliersErrorMessage = string.Empty;

            var (items, total) = await _supplierRepo.GetItemsAsync(SupplierCurrentPage, SupplierPageSize, SearchSupplierKeyword);
            Suppliers = new ObservableCollection<Supplier>(items);
            SupplierTotalItems = total;
        }
        catch (Exception ex)
        {
            SuppliersErrorMessage = $"Error loading suppliers: {ex.Message}";
        }
        finally
        {
            IsLoadingSuppliers = false;
        }
    }

    [RelayCommand]
    private async Task SearchSuppliersAsync()
    {
        SupplierCurrentPage = 1;
        await LoadSuppliersAsync();
    }

    [RelayCommand]
    private async Task NextSupplierPageAsync()
    {
        if (SupplierCurrentPage < SupplierTotalPages)
        {
            SupplierCurrentPage++;
            await LoadSuppliersAsync();
        }
    }

    [RelayCommand]
    private async Task PrevSupplierPageAsync()
    {
        if (SupplierCurrentPage > 1)
        {
            SupplierCurrentPage--;
            await LoadSuppliersAsync();
        }
    }

    [RelayCommand]
    public async Task DeleteSupplierAsync(Supplier supplier)
    {
        if (supplier == null) return;
        try
        {
            await _supplierRepo.DeleteAsync(supplier.Id);
            await LoadSuppliersAsync();
        }
        catch (Exception ex)
        {
            SuppliersErrorMessage = $"Error deleting: {ex.Message}";
        }
    }

    // ─── Supply Orders ────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isLoadingOrders;

    [ObservableProperty]
    private string _ordersErrorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SupplyOrder> _supplyOrders = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderTotalPages))]
    [NotifyPropertyChangedFor(nameof(OrderDisplayFrom))]
    [NotifyPropertyChangedFor(nameof(OrderDisplayTo))]
    private int _orderCurrentPage = 1;

    [ObservableProperty]
    private int _orderPageSize = 20;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderTotalPages))]
    [NotifyPropertyChangedFor(nameof(OrderDisplayFrom))]
    [NotifyPropertyChangedFor(nameof(OrderDisplayTo))]
    private int _orderTotalItems;

    public int OrderTotalPages => (OrderTotalItems + OrderPageSize - 1) / Math.Max(1, OrderPageSize);
    public int OrderDisplayFrom => OrderTotalItems == 0 ? 0 : (OrderCurrentPage - 1) * OrderPageSize + 1;
    public int OrderDisplayTo => Math.Min(OrderCurrentPage * OrderPageSize, OrderTotalItems);

    [RelayCommand]
    public async Task LoadSupplyOrdersAsync()
    {
        try
        {
            IsLoadingOrders = true;
            OrdersErrorMessage = string.Empty;

            var (items, total) = await _supplyRepo.GetOrdersAsync(OrderCurrentPage, OrderPageSize);
            SupplyOrders = new ObservableCollection<SupplyOrder>(items);
            OrderTotalItems = total;
        }
        catch (Exception ex)
        {
            OrdersErrorMessage = $"Error loading supply orders: {ex.Message}";
        }
        finally
        {
            IsLoadingOrders = false;
        }
    }

    [RelayCommand]
    private async Task NextOrderPageAsync()
    {
        if (OrderCurrentPage < OrderTotalPages)
        {
            OrderCurrentPage++;
            await LoadSupplyOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task PrevOrderPageAsync()
    {
        if (OrderCurrentPage > 1)
        {
            OrderCurrentPage--;
            await LoadSupplyOrdersAsync();
        }
    }
}
