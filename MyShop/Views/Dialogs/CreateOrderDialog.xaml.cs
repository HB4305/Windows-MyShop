using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Views.Dialogs;

public class CreateOrderDialogViewModel : INotifyPropertyChanged
{
    private readonly SportItemService _itemService;

    public CustomerOrder Order { get; } = new();
    public ObservableCollection<OrderDetail> OrderDetails { get; } = new();

    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        set
        {
            if (_totalAmount != value)
            {
                _totalAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalAmountDisplay));
            }
        }
    }

    public string TotalAmountDisplay => TotalAmount.ToString("C");

    public CreateOrderDialogViewModel()
    {
        _itemService = App.Services.GetRequiredService<SportItemService>();
        OrderDetails.CollectionChanged += (s, e) => RecalculateTotal();
    }

    public async Task<List<SportItem>> SearchProductsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<SportItem>();
        var result = await _itemService.GetItemsAsync(1, 20, query, null, null, "Name", true); 
        return result.Items.Select(r => r.Item).ToList();
    }

    public void AddProduct(SportItem item)
    {
        var firstVariant = item.Variants.FirstOrDefault();
        var detail = new OrderDetail
        {
            ItemId = item.Id,
            ItemName = item.Name,
            UnitPrice = item.SellingPrice ?? 0m,
            Quantity = 1,
            AvailableVariants = item.Variants,
            LowStockThreshold = item.LowStockThreshold ?? 5
        };
        OrderDetails.Add(detail);
        RecalculateTotal();
    }

    public void RemoveProduct(int itemId)
    {
        var existing = OrderDetails.FirstOrDefault(d => d.ItemId == itemId);
        if (existing != null)
        {
            OrderDetails.Remove(existing);
            RecalculateTotal();
        }
    }

    public void RecalculateTotal()
    {
        TotalAmount = OrderDetails.Sum(d => d.Quantity * d.UnitPrice);
    }

    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(Order.CustomerName))
            return (false, "Please enter Customer Name.");
            
        if (OrderDetails.Count == 0)
            return (false, "Please add at least one item.");
            
        if (Order.OrderType == "Delivery" && string.IsNullOrWhiteSpace(Order.ShippingAddress))
            return (false, "Shipping Address is required for Delivery orders.");

        foreach (var detail in OrderDetails)
        {
            if (detail.SelectedVariant == null)
            {
                if (!string.IsNullOrEmpty(detail.SelectedSize) && !string.IsNullOrEmpty(detail.SelectedColor))
                {
                    return (false, $"The variant for {detail.ItemName} does not exist.");
                }
                return (false, $"Please select a valid variant for {detail.ItemName}.");
            }

            if (detail.SelectedVariant.StockQuantity <= 0)
                return (false, $"{detail.ItemName} is out of stock.");

            if (detail.Quantity > detail.SelectedVariant.StockQuantity)
                return (false, $"Not enough stock for {detail.ItemName}. Max available: {detail.SelectedVariant.StockQuantity}.");
        }

        return (true, string.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed partial class CreateOrderDialog : ContentDialog
{
    public CreateOrderDialogViewModel ViewModel { get; }

    public CreateOrderDialog()
    {
        ViewModel = new CreateOrderDialogViewModel();
        this.InitializeComponent();
        // Initial state set by XAML SelectedIndex="0" will trigger SelectionChanged
    }

    private async void ProductSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var results = await ViewModel.SearchProductsAsync(sender.Text);
            sender.ItemsSource = results;
        }
    }

    private void ProductSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is SportItem selectedItem)
        {
            ViewModel.AddProduct(selectedItem);
            sender.Text = string.Empty;
            sender.ItemsSource = null;
        }
    }

    private void Quantity_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (sender.DataContext is OrderDetail detail)
        {
            if (!double.IsNaN(args.NewValue))
            {
                detail.Quantity = (int)args.NewValue;
            }
            ViewModel.RecalculateTotal();
        }
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is OrderDetail detail)
        {
            ViewModel.OrderDetails.Remove(detail);
            ViewModel.RecalculateTotal();
        }
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var validation = ViewModel.Validate();
        if (!validation.IsValid)
        {
            args.Cancel = true;
            ErrorTextBlock.Text = validation.ErrorMessage;
            ErrorTextBlock.Visibility = Visibility.Visible;
            return;
        }
        ErrorTextBlock.Visibility = Visibility.Collapsed;
    }

    private void OrderTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel == null) return;
        if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
        {
            string type = item.Content.ToString() ?? "AtStore";
            ViewModel.Order.OrderType = type;
            if (CustomerAddressBox != null)
            {
                CustomerAddressBox.Visibility = (type == "Delivery") ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
