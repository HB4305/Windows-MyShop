using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class CustomerViewModel : ObservableObject
{
    private readonly CustomerService _service;
    private readonly SettingsManager _settingsManager;

    public CustomerViewModel(CustomerService service, SettingsManager settingsManager)
    {
        _service = service;
        _settingsManager = settingsManager;
        var savedPageSize = _settingsManager.GetItemsPerPage();
        PageSize = Math.Max(1, savedPageSize);
    }

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Pagination
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _pageSize = 10;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _totalItems;

    public int TotalPages => (TotalItems + PageSize - 1) / Math.Max(1, PageSize);
    public int DisplayFrom => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int DisplayTo => Math.Min(CurrentPage * PageSize, TotalItems);

    // Searching
    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [RelayCommand]
    public async Task LoadCustomersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var (items, totalCount) = await _service.GetCustomersAsync(
                CurrentPage,
                PageSize,
                SearchKeyword);

            Customers = new ObservableCollection<Customer>(items);
            TotalItems = totalCount;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading customers: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(Customer customer)
    {
        if (customer == null) return;
        
        try
        {
            await _service.DeleteCustomerAsync(customer.Id);
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting customer: {ex.Message}";
        }
    }
}
