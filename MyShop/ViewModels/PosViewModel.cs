using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class PosViewModel : ObservableObject
{
    private const decimal TaxRate = 0.08m;
    private const string WalkInCustomerName = "Walk-in Customer";
    private const string WalkInCustomerPhone = "0000000000";
    private const string DefaultPaymentMethod = "Cash";

    private readonly SportItemService _sportItemService;
    private readonly CategoryService _categoryService;
    private readonly CustomerService _customerService;
    private readonly CustomerOrderService _orderService;
    private readonly ShiftService _shiftService;
    private readonly CurrentUserService _currentUserService;
    private readonly SettingsManager _settingsManager;
    private readonly Dictionary<string, int> _categoryIdsByName = new(StringComparer.OrdinalIgnoreCase);
    private List<string> _availableProductOptions = [];
    private bool _isApplyingCustomer;

    public PosViewModel(
        SportItemService sportItemService,
        CategoryService categoryService,
        CustomerService customerService,
        CustomerOrderService orderService,
        ShiftService shiftService,
        CurrentUserService currentUserService,
        SettingsManager settingsManager)
    {
        _sportItemService = sportItemService;
        _categoryService = categoryService;
        _customerService = customerService;
        _orderService = orderService;
        _shiftService = shiftService;
        _currentUserService = currentUserService;
        _settingsManager = settingsManager;

        PageSize = Math.Max(1, _settingsManager.GetItemsPerPage());
        CategoryOptions = ["All Gear"];
        ProductOptions = [];
        CustomerSuggestions = [];
        Products = [];
        CartItems.CollectionChanged += CartItems_CollectionChanged;

        _ = InitializeAsync();
    }

    [ObservableProperty] private ObservableCollection<SportItemListRow> _products;
    [ObservableProperty] private ObservableCollection<PosCartItem> _cartItems = [];
    [ObservableProperty] private List<string> _categoryOptions;
    [ObservableProperty] private List<string> _productOptions;
    [ObservableProperty] private ObservableCollection<Customer> _customerSuggestions;
    [ObservableProperty] private Customer? _selectedCustomer;
    [ObservableProperty] private string _selectedCategory = "All Gear";
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _customerSearchText = string.Empty;
    [ObservableProperty] private string _customerName = WalkInCustomerName;
    [ObservableProperty] private string _customerPhone = WalkInCustomerPhone;
    [ObservableProperty] private string _customerAddress = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isCheckingOut;
    [ObservableProperty] private string _selectedPaymentMethod = DefaultPaymentMethod;
    [ObservableProperty] private string _receivedAmountText = string.Empty;
    [ObservableProperty] private decimal _receivedAmount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _pageSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(DisplayFrom))]
    [NotifyPropertyChangedFor(nameof(DisplayTo))]
    private int _totalItems;

    public int TotalPages => Math.Max(1, (TotalItems + PageSize - 1) / Math.Max(1, PageSize));
    public int DisplayFrom => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int DisplayTo => Math.Min(CurrentPage * PageSize, TotalItems);
    public decimal Subtotal => CartItems.Sum(item => item.LineTotal);
    public decimal Tax => Math.Round(Subtotal * TaxRate, 2);
    public decimal Discount => 0m;
    public decimal Total => Subtotal + Tax - Discount;
    public decimal ChangeAmount => Math.Max(0m, Math.Round(ReceivedAmount - Total, 2));
    public bool IsCashPayment => string.Equals(SelectedPaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase);
    public bool IsReceivedAmountEnough => !IsCashPayment || ReceivedAmount >= Total;
    public int CartCount => CartItems.Sum(item => item.Quantity);
    public bool HasCartItems => CartItems.Count > 0;
    public IReadOnlyList<string> PaymentMethodOptions { get; } = ["Cash", "BankTransfer", "COD"];
    public Func<string, string, bool, Task>? ShowCheckoutResultDialogAsync { get; set; }

    private async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductOptionsAsync();
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var result = await _sportItemService.SearchForPosAsync(
                CurrentPage,
                PageSize,
                Normalize(SearchText),
                ResolveSelectedCategoryName());

            Products = new ObservableCollection<SportItemListRow>(result.Items);
            TotalItems = result.TotalCount;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load POS products: {ex.Message}";
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
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
        {
            return;
        }

        CurrentPage++;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
        {
            return;
        }

        CurrentPage--;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private void AddProduct(SportItemListRow? product)
    {
        if (product is null)
        {
            return;
        }

        var variant = ResolveSellableVariant(product.Item);
        if (variant is null)
        {
            StatusMessage = $"{product.Item.Name} has no in-stock variant available.";
            return;
        }

        var existing = CartItems.FirstOrDefault(item =>
            item.ItemId == product.Item.Id &&
            item.VariantId == variant.Id);

        if (existing is not null)
        {
            if (existing.Quantity >= existing.AvailableStock)
            {
                StatusMessage = $"Only {existing.AvailableStock} available for {existing.Name}.";
                return;
            }

            existing.Quantity++;
            RefreshCartTotals();
            return;
        }

        CartItems.Add(new PosCartItem
        {
            Product = product,
            VariantId = variant.Id,
            VariantSize = variant.Size,
            VariantColor = variant.Color,
            VariantStock = variant.StockQuantity
        });
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void IncreaseQuantity(PosCartItem? item)
    {
        if (item is null)
        {
            return;
        }

        item.Quantity++;
        RefreshCartTotals();
    }

    [RelayCommand]
    private void DecreaseQuantity(PosCartItem? item)
    {
        if (item is null)
        {
            return;
        }

        if (item.Quantity <= 1)
        {
            CartItems.Remove(item);
            return;
        }

        item.Quantity--;
        RefreshCartTotals();
    }

    [RelayCommand]
    private void RemoveCartItem(PosCartItem? item)
    {
        if (item is not null)
        {
            CartItems.Remove(item);
        }
    }

    [RelayCommand]
    private void NewOrder()
    {
        CartItems.Clear();
        SelectedCustomer = null;
        CustomerSearchText = string.Empty;
        CustomerName = WalkInCustomerName;
        CustomerPhone = WalkInCustomerPhone;
        CustomerAddress = string.Empty;
        SelectedPaymentMethod = DefaultPaymentMethod;
        ReceivedAmountText = string.Empty;
        ReceivedAmount = 0m;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (CartItems.Count == 0)
        {
            var message = "Add at least one product before checkout.";
            StatusMessage = message;
            await ShowCheckoutResultAsync("Checkout Failed", message, isSuccess: false);
            return;
        }

        if (!_currentUserService.UserId.HasValue)
        {
            var message = "Please sign in before checkout.";
            StatusMessage = message;
            await ShowCheckoutResultAsync("Checkout Failed", message, isSuccess: false);
            return;
        }

        if (IsCashPayment && !IsReceivedAmountEnough)
        {
            var message = "Received cash is not enough to complete this sale.";
            StatusMessage = message;
            await ShowCheckoutResultAsync("Checkout Failed", message, isSuccess: false);
            return;
        }

        try
        {
            IsCheckingOut = true;
            StatusMessage = string.Empty;

            var activeShift = await _shiftService.GetActiveShiftAsync(_currentUserService.UserId.Value);
            if (activeShift is null)
            {
                var message = "Please open a shift before checkout.";
                StatusMessage = message;
                await ShowCheckoutResultAsync("Checkout Failed", message, isSuccess: false);
                return;
            }

            var customer = await ResolveCheckoutCustomerAsync();
            var paymentMethod = NormalizePaymentMethod(SelectedPaymentMethod);
            var effectiveReceivedAmount = IsCashPayment
                ? ReceivedAmount
                : Total;

            var order = new CustomerOrder
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerPhone = customer.Phone,
                ShippingAddress = customer.Address,
                OrderType = "AtStore",
                Status = "Completed",
                PaymentStatus = "Paid",
                PaymentMethod = paymentMethod,
                ReceivedAmount = effectiveReceivedAmount,
                ShiftId = activeShift.Id,
                Notes = "Created from POS"
            };

            var details = new List<OrderDetail>(CartItems.Count);
            foreach (var item in CartItems)
            {
                if (!item.VariantId.HasValue)
                {
                    throw new InvalidOperationException($"Item '{item.Name}' does not have a sellable variant.");
                }

                details.Add(new OrderDetail
                {
                    ItemId = item.ItemId,
                    ItemName = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    VariantId = item.VariantId.Value,
                    Size = item.VariantSize,
                    Color = item.VariantColor
                });
            }

            var created = await _orderService.CreatePosCheckoutOrderAsync(
                order,
                details,
                _currentUserService.UserId.Value,
                _currentUserService.UserEmail ?? "POS");

            var change = ChangeAmount;
            var successMessage = IsCashPayment
                ? $"Order #{created.Id} checked out successfully. Change: {change:C}."
                : $"Order #{created.Id} checked out successfully.";

            NewOrder();
            await LoadProductsAsync();
            StatusMessage = successMessage;
            await ShowCheckoutResultAsync("Checkout Complete", successMessage, isSuccess: true);
        }
        catch (Exception ex)
        {
            var message = $"Checkout failed: {ex.Message}";
            StatusMessage = message;
            await ShowCheckoutResultAsync("Checkout Failed", message, isSuccess: false);
        }
        finally
        {
            IsCheckingOut = false;
        }
    }

    public void UpdateSearchText(string? value)
    {
        SearchText = value ?? string.Empty;
        FilterProductOptions(SearchText);
    }

    public async Task SearchCustomersAsync(string? value)
    {
        CustomerSearchText = value ?? string.Empty;

        var keyword = Normalize(CustomerSearchText);
        if (!_isApplyingCustomer)
        {
            CustomerName = keyword ?? WalkInCustomerName;
        }

        if (keyword is null)
        {
            SelectedCustomer = null;
            CustomerSuggestions = [];
            return;
        }

        try
        {
            var (customers, _) = await _customerService.SearchCustomersByNameAsync(1, 8, keyword);
            CustomerSuggestions = new ObservableCollection<Customer>(customers);
        }
        catch (Exception ex)
        {
            CustomerSuggestions = [];
            StatusMessage = $"Failed to search customers: {ex.Message}";
        }
    }

    public void SelectCustomer(Customer? customer)
    {
        if (customer is null)
        {
            return;
        }

        SelectedCustomer = customer;
        ApplyCustomerToForm(customer);
        StatusMessage = string.Empty;
    }

    public async Task UseCustomerSearchTextAsNameAsync(string? value)
    {
        var keyword = Normalize(value);
        if (keyword is null)
        {
            return;
        }

        var existing = CustomerSuggestions.FirstOrDefault(customer =>
            string.Equals(customer.Name, keyword, StringComparison.OrdinalIgnoreCase));

        existing ??= await _customerService.GetCustomerByNameAsync(keyword);
        if (existing is not null)
        {
            SelectCustomer(existing);
            return;
        }

        SelectedCustomer = null;
        CustomerSearchText = keyword;
        CustomerName = keyword;
    }

    partial void OnCustomerNameChanged(string value) => ClearSelectedCustomerIfFormChanged();

    partial void OnCustomerPhoneChanged(string value) => ClearSelectedCustomerIfFormChanged();

    partial void OnCustomerAddressChanged(string value) => ClearSelectedCustomerIfFormChanged();

    partial void OnSelectedPaymentMethodChanged(string value)
    {
        OnPropertyChanged(nameof(IsCashPayment));
        OnPropertyChanged(nameof(IsReceivedAmountEnough));
        OnPropertyChanged(nameof(ChangeAmount));
    }

    partial void OnReceivedAmountTextChanged(string value)
    {
        ReceivedAmount = ParseDecimalOrZero(value);
        OnPropertyChanged(nameof(IsReceivedAmountEnough));
        OnPropertyChanged(nameof(ChangeAmount));
    }

    public async Task UpdateCategoryAsync(string? value)
    {
        SelectedCategory = NormalizeCategory(value);
        SearchText = string.Empty;
        await LoadProductOptionsAsync();
        await SearchAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            _categoryIdsByName.Clear();
            foreach (var category in categories)
            {
                if (!string.IsNullOrWhiteSpace(category.Name))
                {
                    _categoryIdsByName[category.Name] = category.Id;
                }
            }

            CategoryOptions = ["All Gear", .. categories.Select(category => category.Name).Where(name => !string.IsNullOrWhiteSpace(name)).OrderBy(name => name)];
        }
        catch
        {
            CategoryOptions = ["All Gear"];
        }
    }

    private async Task LoadProductOptionsAsync()
    {
        try
        {
            _availableProductOptions = await _sportItemService.GetProductNamesByCategoryNameAsync(ResolveSelectedCategoryName());
            FilterProductOptions(SearchText);
        }
        catch
        {
            _availableProductOptions = [];
            ProductOptions = [];
        }
    }

    private int? ResolveSelectedCategoryId()
        => SelectedCategory != "All Gear" && _categoryIdsByName.TryGetValue(SelectedCategory, out var categoryId)
            ? categoryId
            : null;

    private string? ResolveSelectedCategoryName()
        => string.Equals(SelectedCategory, "All Gear", StringComparison.OrdinalIgnoreCase)
            ? null
            : Normalize(SelectedCategory);

    private static string NormalizeCategory(string? value)
        => Normalize(value) ?? "All Gear";

    private void FilterProductOptions(string? value)
    {
        var keyword = Normalize(value);
        ProductOptions = keyword is null
            ? [.. _availableProductOptions]
            : [.. _availableProductOptions.Where(name => name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];
    }

    private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (PosCartItem item in e.OldItems)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (PosCartItem item in e.NewItems)
            {
                item.PropertyChanged += CartItem_PropertyChanged;
            }
        }

        RefreshCartTotals();
    }

    private void CartItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PosCartItem.Quantity) || e.PropertyName == nameof(PosCartItem.LineTotal))
        {
            RefreshCartTotals();
        }
    }

    private async Task ShowCheckoutResultAsync(string title, string message, bool isSuccess)
    {
        if (ShowCheckoutResultDialogAsync is null)
        {
            return;
        }

        try
        {
            await ShowCheckoutResultDialogAsync(title, message, isSuccess);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[POS] Failed to show checkout dialog: {ex.Message}");
        }
    }

    private void RefreshCartTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(Discount));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(ChangeAmount));
        OnPropertyChanged(nameof(IsReceivedAmountEnough));
        OnPropertyChanged(nameof(CartCount));
        OnPropertyChanged(nameof(HasCartItems));
    }

    private async Task<Customer> ResolveCheckoutCustomerAsync()
    {
        var name = Normalize(CustomerName) ?? Normalize(CustomerSearchText) ?? WalkInCustomerName;
        var phone = Normalize(CustomerPhone) ?? WalkInCustomerPhone;
        var address = Normalize(CustomerAddress);

        if (SelectedCustomer is not null
            && string.Equals(SelectedCustomer.Name, name, StringComparison.Ordinal)
            && string.Equals(SelectedCustomer.Phone, phone, StringComparison.Ordinal)
            && string.Equals(SelectedCustomer.Address ?? string.Empty, address ?? string.Empty, StringComparison.Ordinal))
        {
            return SelectedCustomer;
        }

        if (HasDefaultCustomerDetails(phone, address))
        {
            var existingByName = await _customerService.GetCustomerByNameAsync(name);
            if (existingByName is not null)
            {
                SelectedCustomer = existingByName;
                ApplyCustomerToForm(existingByName);
                return existingByName;
            }
        }

        if (!string.Equals(phone, WalkInCustomerPhone, StringComparison.Ordinal))
        {
            var existingByPhone = await _customerService.GetCustomerByPhoneAsync(phone);
            if (existingByPhone is not null)
            {
                SelectedCustomer = existingByPhone;
                ApplyCustomerToForm(existingByPhone);
                return existingByPhone;
            }
        }

        var createdCustomer = new Customer
        {
            Name = name,
            Phone = phone,
            Address = address
        };

        createdCustomer.Id = await _customerService.SaveCustomerAsync(createdCustomer);
        SelectedCustomer = createdCustomer;
        ApplyCustomerToForm(createdCustomer);
        return createdCustomer;
    }

    private void ApplyCustomerToForm(Customer customer)
    {
        _isApplyingCustomer = true;
        CustomerSearchText = customer.Name;
        CustomerName = customer.Name;
        CustomerPhone = customer.Phone;
        CustomerAddress = customer.Address ?? string.Empty;
        _isApplyingCustomer = false;
    }

    private void ClearSelectedCustomerIfFormChanged()
    {
        if (_isApplyingCustomer || SelectedCustomer is null)
        {
            return;
        }

        var address = Normalize(CustomerAddress) ?? string.Empty;
        if (!string.Equals(SelectedCustomer.Name, CustomerName, StringComparison.Ordinal)
            || !string.Equals(SelectedCustomer.Phone, CustomerPhone, StringComparison.Ordinal)
            || !string.Equals(SelectedCustomer.Address ?? string.Empty, address, StringComparison.Ordinal))
        {
            SelectedCustomer = null;
        }
    }

    private static bool HasDefaultCustomerDetails(string phone, string? address)
        => string.Equals(phone, WalkInCustomerPhone, StringComparison.Ordinal)
            && string.IsNullOrWhiteSpace(address);

    private static SportItemVariant? ResolveSellableVariant(SportItem item)
        => item.Variants
            .Where(variant => variant.StockQuantity > 0)
            .OrderBy(variant => variant.Id)
            .FirstOrDefault();

    private string NormalizePaymentMethod(string? value)
        => PaymentMethodOptions.FirstOrDefault(option =>
            string.Equals(option, value, StringComparison.OrdinalIgnoreCase))
            ?? DefaultPaymentMethod;

    private static decimal ParseDecimalOrZero(string? value)
    {
        var trimmed = Normalize(value);
        if (trimmed is null)
        {
            return 0m;
        }

        if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureValue))
        {
            return currentCultureValue;
        }

        if (decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        return 0m;
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
