using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class OrdersManagementPage : Page
{
    private readonly CustomerOrderViewModel _vm;

    public OrdersManagementPage()
    {
        this.InitializeComponent();
        _vm = App.Services.GetRequiredService<CustomerOrderViewModel>();
        DataContext = _vm;

        Loaded += OnLoaded;
        OrderListView.SelectionChanged += OrderList_SelectionChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _vm.LoadOrdersAsync();
        BuildPagination();
    }

    private async void OrderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OrderListView.SelectedItem is CustomerOrder order)
            await _vm.SelectOrderAsync(order);
    }

    private void CloseDetail_Click(object sender, RoutedEventArgs e)
        => _vm.CloseDetailPanel();

    // ── Tab clicks ──────────────────────────────────────────────────
    private void TabAll_Click(object sender, RoutedEventArgs e) => SwitchTab("All");
    private void TabPending_Click(object sender, RoutedEventArgs e) => SwitchTab("Pending");
    private void TabProcessing_Click(object sender, RoutedEventArgs e) => SwitchTab("Processing");
    private void TabShipped_Click(object sender, RoutedEventArgs e) => SwitchTab("Shipped");
    private void TabReturned_Click(object sender, RoutedEventArgs e) => SwitchTab("Cancelled");

    private void SwitchTab(string tab)
    {
        _vm.ActiveTab = tab;
        OrderListView.SelectedItem = null;
        BuildPagination();
    }

    // ── Action buttons ──────────────────────────────────────────────
    private void NewSale_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to New Sale / Create Order page
    }

    private async void OrderManifest_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedOrder == null || _vm.SelectedOrder.Id == 0)
        {
            var errDialog = new ConfirmationDialog(
                "No Order Selected",
                "Please select an order from the list before generating an invoice.");
            errDialog.XamlRoot = XamlRoot;
            await errDialog.ShowAsync();
            return;
        }

        var xamlRoot = XamlRoot;
        var dialogVm = new InvoiceDialogViewModel(
            _vm.SelectedOrder,
            _vm.CurrentDetails,
            App.Services.GetRequiredService<IInvoiceService>(),
            App.Services.GetRequiredService<IFilePickerService>(),
            xamlRoot);

        string result = await dialogVm.ExportAsync();

        // Give the UI thread time to fully settle after Export dialog and File Picker
        await Task.Delay(500);

        if (string.IsNullOrEmpty(result)) return;
        
        DispatcherQueue.TryEnqueue(async () =>
        {
            var successDialog = new SuccessDialog("Invoice Export", result);
            successDialog.XamlRoot = xamlRoot;
            await successDialog.ShowAsync();
        });
    }

    private void FulfillGear_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedOrder == null) return;
        // Mark as Shipped / Processing
        _ = _vm.UpdateOrderStatusAsync("Processing");
    }

    // ── Pagination ──────────────────────────────────────────────────
    private void BuildPagination()
    {
        PaginationPanel.Children.Clear();

        var total = _vm.TotalPages;
        var current = _vm.CurrentPage;

        void AddPageBtn(int page, bool isActive)
        {
            var btn = new Button
            {
                Content = page.ToString(),
                MinWidth = 32,
                Height = 32,
                Padding = new Thickness(8, 4, 8, 4),
                FontSize = 13,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(2, 0, 2, 0),
            };

            if (isActive)
            {
                btn.Style = (Style)Application.Current.Resources["PageBtnActive"];
            }
            else
            {
                btn.Style = (Style)Application.Current.Resources["PageBtn"];
            }

            btn.Click += (s, e) =>
            {
                _vm.GoToPageCommand.Execute(page);
                BuildPagination();
            };

            PaginationPanel.Children.Add(btn);
        }

        // Prev button
        var prevBtn = new Button
        {
            Content = new TextBlock { Text = "Prev", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Application.Current.Resources["PageBtn"],
        };
        prevBtn.Click += (s, e) =>
        {
            if (current > 1)
            {
                _vm.GoToPageCommand.Execute(current - 1);
                BuildPagination();
            }
        };
        PaginationPanel.Children.Add(prevBtn);

        // Page numbers
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) PaginationPanel.Children.Add(new TextBlock
            {
                Text = "…",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0),
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondary"],
            });

            for (int i = Math.Max(2, current - 1); i <= Math.Min(total - 1, current + 1); i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) PaginationPanel.Children.Add(new TextBlock
            {
                Text = "…",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0),
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondary"],
            });

            AddPageBtn(total, current == total);
        }

        // Next button
        var nextBtn = new Button
        {
            Content = new TextBlock { Text = "Next", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Application.Current.Resources["PageBtn"],
        };
        nextBtn.Click += (s, e) =>
        {
            if (current < total)
            {
                _vm.GoToPageCommand.Execute(current + 1);
                BuildPagination();
            }
        };
        PaginationPanel.Children.Add(nextBtn);
    }
}
