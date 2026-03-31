using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;
using System.IO;
using System.Collections.ObjectModel;

namespace MyShop.ViewModels;

public partial class SportItemDetailViewModel : ObservableObject
{
    private readonly SportItemService _service;
    private readonly CategoryService _catService;

    public SportItemDetailViewModel(SportItemService service, CategoryService catService)
    {
        _service = service;
        _catService = catService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    private SportItem _item = new();

    public bool IsEditMode => Item.Id != 0;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> _imageUrls = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public async Task InitializeAsync(SportItem? item = null)
    {
        try
        {
            IsLoading = true;
            // Load categories for dropdown
            var cats = await _catService.GetAllAsync();
            Categories = new ObservableCollection<Category>(cats);

            if (item != null)
            {
                Item = item;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == Item.CategoryId);
                
                // Load images for existing product
                ImageUrls.Clear();
                if (Item.ImageUrls != null)
                {
                    foreach (var url in Item.ImageUrls)
                    {
                        ImageUrls.Add(url);
                    }
                }
            }
            else
            {
                Item = new SportItem();
                ImageUrls.Clear();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khởi tạo: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event Action? SaveCompleted;

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            if (SelectedCategory != null) Item.CategoryId = SelectedCategory.Id;
            
            // Đồng bộ danh sách ảnh từ UI vào Model trước khi lưu
            Item.ImageUrls = ImageUrls.ToList();

            if (Item.Id == 0) 
            {
                var newId = await _service.AddAsync(Item);
                Item.Id = newId;
            }
            else 
            {
                await _service.UpdateAsync(Item);
            }
            
            // Lưu thành công → thông báo cho View quay lại
            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi lưu: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        try
        {
            IsLoading = true;
            if (Item.Id != 0)
            {
                await _service.DeleteAsync(Item.Id);
                SaveCompleted?.Invoke();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                using var stream = await file.OpenStreamForReadAsync();
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);

                // Tải lên Storage
                var publicUrl = await _service.UploadImageAsync(buffer, file.Name);
                
                // Cập nhật lại danh sách hiển thị (việc lưu vào DB sẽ được thực hiện khi nhấn nút Save chung)
                ImageUrls.Add(publicUrl);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải ảnh: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void DeleteImage(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        ImageUrls.Remove(url);
    }
}
