using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class SportItemDetailViewModel : ObservableObject
{
    private readonly SportItemService _service;
    private readonly CategoryService _catService;

    public Func<string, string, Task<bool>>? ShowConfirmationDialogAsync { get; set; }

    public SportItemDetailViewModel(SportItemService service, CategoryService catService)
    {
        _service = service;
        _catService = catService;
        ImageUrls.CollectionChanged += OnImageUrlsCollectionChanged;
    }

    private void OnImageUrlsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
                if (e.OldStartingIndex >= 0 && e.OldStartingIndex < SelectedImageIndex)
                    SelectedImageIndex--;
                else if (e.OldStartingIndex >= 0
                         && e.OldStartingIndex == SelectedImageIndex
                         && SelectedImageIndex >= ImageUrls.Count)
                    SelectedImageIndex = Math.Max(0, ImageUrls.Count - 1);
                break;
            case NotifyCollectionChangedAction.Reset:
                SelectedImageIndex = 0;
                break;
            case NotifyCollectionChangedAction.Move:
                AdjustSelectedIndexAfterMove(e.OldStartingIndex, e.NewStartingIndex);
                break;
        }

        if (ImageUrls.Count == 0)
            SelectedImageIndex = 0;
        else if (SelectedImageIndex >= ImageUrls.Count)
            SelectedImageIndex = ImageUrls.Count - 1;

        OnPropertyChanged(nameof(PreviewImageUrl));
    }

    /// <summary>Keeps selection aligned when items reorder (drag-drop or Move).</summary>
    private void AdjustSelectedIndexAfterMove(int oldIdx, int newIdx)
    {
        if (oldIdx < 0 || newIdx < 0) return;
        if (SelectedImageIndex == oldIdx)
            SelectedImageIndex = newIdx;
        else if (oldIdx < newIdx && oldIdx < SelectedImageIndex && SelectedImageIndex <= newIdx)
            SelectedImageIndex--;
        else if (oldIdx > newIdx && newIdx <= SelectedImageIndex && SelectedImageIndex < oldIdx)
            SelectedImageIndex++;
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
    [NotifyPropertyChangedFor(nameof(PreviewImageUrl))]
    private int _selectedImageIndex;

    /// <summary>URL of the image shown in the main preview (left).</summary>
    public string? PreviewImageUrl =>
        ImageUrls.Count > 0 && SelectedImageIndex >= 0 && SelectedImageIndex < ImageUrls.Count
            ? ImageUrls[SelectedImageIndex]
            : null;

    /// <summary>Description field on the form (no DB column yet).</summary>
    [ObservableProperty]
    private string _productDescriptionUi = string.Empty;

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
                ProductDescriptionUi = string.Empty;
            }

            SelectedImageIndex = ImageUrls.Count > 0 ? 0 : 0;
            OnPropertyChanged(nameof(PreviewImageUrl));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Initialization error: {ex.Message}";
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
            
            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Item.Id == 0 || ShowConfirmationDialogAsync is null)
        {
            return;
        }

        bool confirmed = await ShowConfirmationDialogAsync(
            "Confirm delete",
            $"Are you sure you want to delete \"{Item.Name}\"?\nThis action cannot be undone.");

        if (!confirmed)
        {
            return;
        }

        try
        {
            IsLoading = true;
            await _service.DeleteAsync(Item.Id);
            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Delete error: {ex.Message}";
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

                var publicUrl = await _service.UploadImageAsync(buffer, file.Name);
                ImageUrls.Add(publicUrl);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Image upload error: {ex.Message}";
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

    [RelayCommand]
    private void MoveSelectedImageEarlier()
    {
        if (SelectedImageIndex <= 0 || SelectedImageIndex >= ImageUrls.Count) return;
        ImageUrls.Move(SelectedImageIndex, SelectedImageIndex - 1);
    }

    [RelayCommand]
    private void MoveSelectedImageLater()
    {
        if (SelectedImageIndex < 0 || SelectedImageIndex >= ImageUrls.Count - 1) return;
        ImageUrls.Move(SelectedImageIndex, SelectedImageIndex + 1);
    }
}
