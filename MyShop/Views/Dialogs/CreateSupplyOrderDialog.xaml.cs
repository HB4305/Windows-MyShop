using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Views.Dialogs;

public class SupplyDetailItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;

    public long? VariantId { get; set; }
    public List<SportItemVariant> AvailableVariants { get; set; } = new();

    public List<string> AvailableSizes => AvailableVariants
        .Select(v => v.Size ?? "N/A")
        .Distinct()
        .OrderBy(s => s)
        .ToList();

    public List<string> AvailableColors => AvailableVariants
        .Select(v => v.Color ?? "N/A")
        .Distinct()
        .OrderBy(c => c)
        .ToList();

    private string? _selectedSize;
    public string? SelectedSize
    {
        get => _selectedSize;
        set
        {
            if (_selectedSize != value)
            {
                _selectedSize = value;
                OnPropertyChanged();
                UpdateSelectedVariant();
            }
        }
    }

    private string? _selectedColor;
    public string? SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor != value)
            {
                _selectedColor = value;
                OnPropertyChanged();
                UpdateSelectedVariant();
            }
        }
    }

    private void UpdateSelectedVariant()
    {
        var matched = AvailableVariants.FirstOrDefault(v =>
            (v.Size ?? "N/A") == (SelectedSize ?? "N/A") &&
            (v.Color ?? "N/A") == (SelectedColor ?? "N/A")
        );

        if (matched != null)
        {
            VariantId = matched.Id;
        }
        else
        {
            VariantId = null;
        }
    }

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalPriceDisplay));
            }
        }
    }

    private decimal _importPrice = 0;
    public decimal ImportPrice
    {
        get => _importPrice;
        set
        {
            if (_importPrice != value)
            {
                _importPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalPriceDisplay));
            }
        }
    }

    public decimal TotalPrice => Quantity * ImportPrice;
    
    public string TotalPriceDisplay => TotalPrice.ToString("C", new CultureInfo("en-US"));
}

public sealed partial class CreateSupplyOrderDialog : ContentDialog
{
    private readonly SupplierRepository _supplierRepo;
    private readonly SportItemRepository _sportItemRepo;
    private readonly SupplyRepository _supplyRepo;
    private ContentDialogResult _result = ContentDialogResult.None;

    public ObservableCollection<SupplyDetailItem> SelectedItems { get; set; } = new();

    public CreateSupplyOrderDialog()
    {
        this.InitializeComponent();
        
        _supplierRepo = App.Services.GetRequiredService<SupplierRepository>();
        _sportItemRepo = App.Services.GetRequiredService<SportItemRepository>();
        _supplyRepo = App.Services.GetRequiredService<SupplyRepository>();

        ItemsListView.ItemsSource = SelectedItems;
        SelectedItems.CollectionChanged += (s, e) => UpdateTotalCost();

        Loaded += CreateSupplyOrderDialog_Loaded;
    }

    public new async Task<ContentDialogResult> ShowAsync()
    {
        await base.ShowAsync();
        return _result;
    }

    private async void CreateSupplyOrderDialog_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var suppliers = await _supplierRepo.GetAllAsync();
            SupplierComboBox.ItemsSource = suppliers;
            if (suppliers.Count > 0)
                SupplierComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ShowError($"Error loading data: {ex.Message}");
        }
    }

    private async void ProductSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var keyword = sender.Text;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                sender.ItemsSource = null;
                return;
            }

            try
            {
                var (items, _) = await _sportItemRepo.GetItemsAsync(1, 10, keyword, null, null, "name", true);
                sender.ItemsSource = items.Select(r => r.Item).ToList();
            }
            catch
            {
                // Ignore search errors
            }
        }
    }

    private void ProductSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is SportItem product)
        {
            sender.Text = string.Empty;

            var defaultVariant = product.Variants.FirstOrDefault();
            var newItem = new SupplyDetailItem
            {
                ItemId = product.Id,
                ItemName = product.Name,
                Quantity = 1,
                ImportPrice = product.CostPrice ?? 0,
                AvailableVariants = product.Variants
            };
            
            if (defaultVariant != null)
            {
                newItem.SelectedSize = defaultVariant.Size ?? "N/A";
                newItem.SelectedColor = defaultVariant.Color ?? "N/A";
            }
            
            SelectedItems.Add(newItem);
        }
    }

    private void ProductSearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        SearchWrapper.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AppPurpleBrush"];
    }

    private void ProductSearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        SearchWrapper.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        UpdateTotalCost();
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is SupplyDetailItem item)
        {
            SelectedItems.Remove(item);
        }
    }

    private void UpdateTotalCost()
    {
        decimal total = SelectedItems.Sum(x => x.TotalPrice);
        TotalCostText.Text = total.ToString("C", new CultureInfo("en-US"));
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        _result = ContentDialogResult.None;
        Hide();
    }

    private async void CreateBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var selectedSupplier = SupplierComboBox.SelectedItem as Supplier;
            if (selectedSupplier == null)
            {
                ShowError("Please select a supplier.");
                return;
            }

            if (SelectedItems.Count == 0)
            {
                ShowError("Please add at least 1 product to the supply order.");
                return;
            }

            decimal totalCost = SelectedItems.Sum(x => x.TotalPrice);

            var order = new SupplyOrder
            {
                SupplierId = selectedSupplier.Id,
                ImportDate = DateTime.Now,
                TotalCost = totalCost
            };

            var details = SelectedItems.Select(item => new SupplyDetail
            {
                ItemId = item.ItemId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                ImportPrice = item.ImportPrice
            }).ToList();

            await _supplyRepo.CreateSupplyOrderTransactionAsync(order, details);
            
            _result = ContentDialogResult.Primary;
            Hide();
        }
        catch (Exception ex)
        {
            ShowError($"Error creating supply order: {ex.Message}");
        }
    }
}
