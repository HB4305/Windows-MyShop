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
                // Search for products matching keyword
                var (items, _) = await _sportItemRepo.GetItemsAsync(1, 10, keyword, null, null, "name", true);
                sender.ItemsSource = items.Select(r => r.Item).ToList();
            }
            catch
            {
                // Ignore search errors to avoid popup spam
            }
        }
    }

    private void ProductSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is SportItem product)
        {
            // Clear text
            sender.Text = string.Empty;

            // Check if already in list with SAME VARIANT? Actually, we don't know the variant yet.
            // Just add a new row and let them pick.
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

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        try
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var selectedSupplier = SupplierComboBox.SelectedItem as Supplier;
            if (selectedSupplier == null)
            {
                ShowError("Please select a supplier.");
                args.Cancel = true;
                return;
            }

            if (SelectedItems.Count == 0)
            {
                ShowError("Please add at least 1 product to the supply order.");
                args.Cancel = true;
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

            try
            {
                await _supplyRepo.CreateSupplyOrderTransactionAsync(order, details);
            }
            catch (Exception ex)
            {
                ShowError($"Error creating supply order: {ex.Message}");
                args.Cancel = true;
            }
        }
        finally
        {
            deferral.Complete();
        }
    }
}
