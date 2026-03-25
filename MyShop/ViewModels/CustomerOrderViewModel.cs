
using System.Collections.ObjectModel;
using MyShop.Models;
using MyShop.Services;


namespace MyShop.ViewModels;

public partial class CustomerOrderViewModel : ObservableObject
{
    private readonly CustomerOrderService _service;

    public CustomerOrderViewModel(CustomerOrderService service)
    {
        _service = service;
        // Tự động tải danh sách khi khởi tạo
        _ = LoadCustomerOrdersAsync();
    }

    [ObservableProperty]
    private ObservableCollection<CustomerOrder> _customerOrders = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    public async Task LoadCustomerOrdersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _service.GetAllAsync();
            CustomerOrders = new ObservableCollection<CustomerOrder>(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }


}