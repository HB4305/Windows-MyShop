using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.Models;
using MyShop.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyShop.Views.Dialogs;

public class CreateOrderDialogViewModel : INotifyPropertyChanged
{
    private readonly SportItemService _itemService;

    public CustomerOrder Order { get; } = new();
    public ObservableCollection<OrderDetail> OrderDetails { get; } = new();
    public ObservableCollection<string> AvailablePaymentMethods { get; } = new();

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
        UpdateAvailablePaymentMethods("AtStore"); // Default
    }
 
    public void UpdateAvailablePaymentMethods(string orderType)
    {
        AvailablePaymentMethods.Clear();
        if (orderType == "AtStore")
        {
            AvailablePaymentMethods.Add("Cash");
            AvailablePaymentMethods.Add("BankTransfer");
            AvailablePaymentMethods.Add("CreditCard");
            if (Order.PaymentMethod == "COD") Order.PaymentMethod = "Cash";
        }
        else // Delivery
        {
            AvailablePaymentMethods.Add("COD");
            AvailablePaymentMethods.Add("BankTransfer");
            if (Order.PaymentMethod == "Cash" || Order.PaymentMethod == "CreditCard") 
                Order.PaymentMethod = "COD";
        }
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
    private readonly Dictionary<DependencyObject, (Brush? BorderBrush, Thickness BorderThickness)> _focusedControlStates = new();

    public CreateOrderDialog()
    {
        ViewModel = new CreateOrderDialogViewModel();
        this.InitializeComponent();
        this.DataContext = ViewModel;

        // Initialize defaults to match UI selection
        ViewModel.Order.OrderType = "AtStore";
        if (string.IsNullOrEmpty(ViewModel.Order.PaymentMethod))
        {
            ViewModel.Order.PaymentMethod = "Cash";
        }

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

    private Border? GetFocusBorder(object sender)
    {
        return sender switch
        {
            TextBox textBox => textBox.Parent as Border,
            AutoSuggestBox searchBox => searchBox.Parent as Border,
            NumberBox numberBox => numberBox.Parent as Border,
            _ => null
        };
    }

    private void OrderTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel == null) return;

        if (OrderTypeCombo.SelectedItem is ComboBoxItem item)
        {
            var typeStr = item.Content.ToString() ?? "AtStore";
            ViewModel.Order.OrderType = typeStr;
            ViewModel.UpdateAvailablePaymentMethods(typeStr);
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

    private void Input_GotFocus(object sender, RoutedEventArgs e)
    {
        var focusBorder = GetFocusBorder(sender);
        if (focusBorder != null)
        {
            if (!_focusedControlStates.ContainsKey(focusBorder))
            {
                _focusedControlStates[focusBorder] = (focusBorder.BorderBrush, focusBorder.BorderThickness);
            }

            if (Application.Current.Resources.TryGetValue("PurpleBrush", out var purpleObj) && purpleObj is Brush purple)
            {
                focusBorder.BorderBrush = purple;
            }

            focusBorder.BorderThickness = new Thickness(1);
            return;
        }

        if (sender is not Control control)
        {
            return;
        }

        if (!_focusedControlStates.ContainsKey(control))
        {
            _focusedControlStates[control] = (control.BorderBrush, control.BorderThickness);
        }

        if (Application.Current.Resources.TryGetValue("PurpleBrush", out var purpleObj2) && purpleObj2 is Brush purple2)
        {
            control.BorderBrush = purple2;
        }

        control.BorderThickness = new Thickness(1);
    }

    private void Input_LostFocus(object sender, RoutedEventArgs e)
    {
        var focusBorder = GetFocusBorder(sender);
        if (focusBorder != null)
        {
            if (_focusedControlStates.TryGetValue(focusBorder, out var state))
            {
                focusBorder.BorderBrush = state.BorderBrush;
                focusBorder.BorderThickness = state.BorderThickness;
            }
            return;
        }

        if (sender is Control control && _focusedControlStates.TryGetValue(control, out var state2))
        {
            control.BorderBrush = state2.BorderBrush;
            control.BorderThickness = state2.BorderThickness;
        }
    }

    private void Quantity_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        ViewModel.RecalculateTotal();
    }

    private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
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
        // Clear all validation errors first
        ClearAllValidationErrors();
        bool hasError = false;

        // 1. Basic Customer Info
        if (string.IsNullOrWhiteSpace(ViewModel.Order.CustomerName))
        {
            ShowFieldError(CustomerNameErrorBorder, CustomerNameErrorText, "Name is required");
            hasError = true;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Order.CustomerPhone))
        {
            ShowFieldError(CustomerPhoneErrorBorder, CustomerPhoneErrorText, "Phone Number is required");
            hasError = true;
        }

        // 2. Order Metadata
        if (string.IsNullOrWhiteSpace(ViewModel.Order.OrderType))
        {
            ShowFieldError(OrderTypeErrorBorder, OrderTypeErrorText, "Order Type is required");
            hasError = true;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Order.PaymentMethod))
        {
            ShowFieldError(PaymentMethodErrorBorder, PaymentMethodErrorText, "Payment Method is required");
            hasError = true;
        }

        // 3. Shipping Address for Delivery
        if (ViewModel.Order.OrderType == "Delivery" && string.IsNullOrWhiteSpace(ViewModel.Order.ShippingAddress))
        {
            ShowFieldError(CustomerAddressErrorBorder, CustomerAddressErrorText, "Address is required for Delivery");
            hasError = true;
        }

        if (hasError) return;

        // 4. Order Items
        if (ViewModel.OrderDetails.Count == 0)
        {
            ShowFieldError(OrderItemsErrorBorder, OrderItemsErrorText, "Please add at least one product");
            return;
        }

        // 5. Item Variants (Size/Color)
        foreach (var detail in ViewModel.OrderDetails)
        {
            if (string.IsNullOrWhiteSpace(detail.SelectedSize) || string.IsNullOrWhiteSpace(detail.SelectedColor))
            {
                ShowFieldError(OrderItemsErrorBorder, OrderItemsErrorText, $"Missing Size/Color for '{detail.ItemName}'");
                return;
            }
        }

        IsSubmitted = true;
        _result = ContentDialogResult.Primary;
        Hide();
    }

    private void ClearAllValidationErrors()
    {
        OrderTypeErrorBorder.Visibility = Visibility.Collapsed;
        PaymentMethodErrorBorder.Visibility = Visibility.Collapsed;
        CustomerNameErrorBorder.Visibility = Visibility.Collapsed;
        CustomerPhoneErrorBorder.Visibility = Visibility.Collapsed;
        CustomerAddressErrorBorder.Visibility = Visibility.Collapsed;
        OrderItemsErrorBorder.Visibility = Visibility.Collapsed;
    }

    private void ShowFieldError(Border errorBorder, TextBlock errorText, string message)
    {
        errorBorder.Visibility = Visibility.Visible;
        errorText.Text = message;
    }

}
