using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public partial class OrderDetailViewModel : ObservableObject
{
    private readonly OrderDetailService _service;

    public OrderDetailViewModel(OrderDetailService detailService)
    {
        _service = detailService;
    }

    [ObservableProperty]
    private ObservableCollection<OrderDetail> _details = new();

    [ObservableProperty]
    private OrderDetail _selectedDetail = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    public async Task LoadDetailsByOrderIdAsync(int orderId)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var details = await _service.GetDetailsByOrderIdAsync(orderId);
            Details = new ObservableCollection<OrderDetail>(details);
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
    public async Task CreateDetailAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await _service.CreateDetailAsync(SelectedDetail);
            SelectedDetail = new();
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
    public async Task UpdateDetailAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await _service.UpdateDetailAsync(SelectedDetail);
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
    public async Task DeleteDetailAsync(int id)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await _service.DeleteDetailAsync(id);
            var detail = Details.FirstOrDefault(d => d.Id == id);
            if (detail != null) Details.Remove(detail);
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
}