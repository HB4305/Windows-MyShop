using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public CreateOrderDialogViewModel()
    {
        _itemService = App.Services.GetRequiredService<SportItemService>();
        OrderDetails.CollectionChanged += (s, e) => RecalculateTotal();
    }

    public async Task<IEnumerable<SportItem>> SearchProductsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return Enumerable.Empty<SportItem>();
        var (items, _) = await _itemService.GetItemsAsync(1, 10, keyword, null, null, "name", true);
        return items.Select(r => r.Item);
    }

    public void AddProductToOrder(SportItem product)
    {
        var detail = new OrderDetail
        {
            ItemId = product.Id,
            ItemName = product.Name,
            UnitPrice = product.SellingPrice ?? 0,
            Quantity = 1,
            AvailableVariants = product.Variants
        };

        var firstVariant = product.Variants.FirstOrDefault();
        if (firstVariant != null)
        {
            detail.SelectedSize = firstVariant.Size ?? "N/A";
            detail.SelectedColor = firstVariant.Color ?? "N/A";
        }

        OrderDetails.Add(detail);
        RecalculateTotal();
    }

    public void RemoveProductFromOrder(OrderDetail detail)
    {
        OrderDetails.Remove(detail);
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        TotalAmount = OrderDetails.Sum(d => (decimal)d.Quantity * d.UnitPrice);
    }
}

public sealed partial class CreateOrderDialog : ContentDialog
{
    public CreateOrderDialogViewModel ViewModel { get; }
    public bool IsSubmitted { get; private set; }
    private ContentDialogResult _result = ContentDialogResult.None;

    public CreateOrderDialog()
    {
        ViewModel = new CreateOrderDialogViewModel();
        this.InitializeComponent();
        this.DataContext = ViewModel;

        Loaded += CreateOrderDialog_Loaded;
    }

    public new async Task<ContentDialogResult> ShowAsync()
    {
        await base.ShowAsync();
        return _result;
    }

    private void CreateOrderDialog_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateAddressVisibility();
    }

    private void OrderTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel == null) return;

        if (OrderTypeCombo.SelectedItem is ComboBoxItem item)
        {
            var typeStr = item.Content.ToString() ?? "AtStore";
            ViewModel.Order.OrderType = typeStr;
            UpdateAddressVisibility();
        }
    }

    private void UpdateAddressVisibility()
    {
        if (CustomerAddressBox == null) return;
        CustomerAddressBox.Visibility = (ViewModel.Order.OrderType == "Delivery") ? Visibility.Visible : Visibility.Collapsed;
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
        if (args.SelectedItem is SportItem product)
        {
            ViewModel.AddProductToOrder(product);
            sender.Text = string.Empty;
        }
    }

    private void Quantity_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        ViewModel.RecalculateTotal();
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is OrderDetail detail)
        {
            ViewModel.RemoveProductFromOrder(detail);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsSubmitted = false;
        _result = ContentDialogResult.None;
        Hide();
    }

    private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorBorder.Visibility = Visibility.Collapsed;

        if (string.IsNullOrWhiteSpace(ViewModel.Order.CustomerName))
        {
            ShowError("Customer Name is required.");
            return;
        }

        if (ViewModel.Order.OrderType == "Delivery" && string.IsNullOrWhiteSpace(ViewModel.Order.ShippingAddress))
        {
            ShowError("Shipping Address is required for Delivery orders.");
            return;
        }

        if (ViewModel.OrderDetails.Count == 0)
        {
            ShowError("Please add at least one product to the order.");
            return;
        }

        IsSubmitted = true;
        _result = ContentDialogResult.Primary;
        Hide();
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }
}
