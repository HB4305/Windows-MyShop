using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class CategoryViewModel : ObservableObject
{
    private readonly CategoryService _service;

    public CategoryViewModel(CategoryService service)
    {
        _service = service;
        // Tự động tải danh sách khi khởi tạo
        _ = LoadCategoriesAsync();
    }

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _service.GetAllAsync();
            Categories = new ObservableCollection<Category>(result);
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
