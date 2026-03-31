using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public partial class CustomerOrderViewModel : ObservableObject
{
    private readonly CustomerOrderService _service;

    public CustomerOrderViewModel(CustomerOrderService orderService)
    {
        _service = orderService;
    }

    [ObservableProperty]
    private ObservableCollection<CustomerOrder> _orders = new();

    [ObservableProperty]
    private CustomerOrder _selectedOrder = new();

    [ObservableProperty]
    private ObservableCollection<OrderDetail> _currentDetails = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var orders = await _service.GetAllOrdersAsync();
            Orders = new ObservableCollection<CustomerOrder>(orders);
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

    [RelayCommand]
    public async Task LoadOrderByIdAsync(int id)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            SelectedOrder = await _service.GetOrderByIdAsync(id) ?? new();
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

    [RelayCommand]
    public async Task CreateOrderAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await _service.CreateOrderAsync(SelectedOrder, CurrentDetails.ToList());
            await LoadOrdersAsync();
            SelectedOrder = new();
            CurrentDetails.Clear();
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

    [RelayCommand]
    public async Task UpdateOrderAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await _service.UpdateOrderAsync(SelectedOrder, CurrentDetails.ToList());
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

    [RelayCommand]
    public async Task DeleteOrderAsync(int id)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await _service.DeleteOrderAsync(id);
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

    public void SelectOrder(CustomerOrder order, List<OrderDetail> details)
    {
        SelectedOrder = order;
        CurrentDetails = new ObservableCollection<OrderDetail>(details);
    }
}