using Microsoft.Extensions.DependencyInjection;
using MyShop.Models;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace MyShop.Views;

public sealed partial class CustomerOrderPage : Page
{
    public CustomerOrderViewModel ViewModel { get; }

    public CustomerOrderPage()
    {
        this.InitializeComponent();
        try
        {
            ViewModel = App.Services.GetRequiredService<CustomerOrderViewModel>();
            DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                await ViewModel.LoadOrdersCommand.ExecuteAsync(null);
                BuildPaginationButtons();
                UpdateTabStyles();
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CustomerOrderPage] Failed to initialize: {ex.Message}");
            throw;
        }

        // Rebuild pagination when page changes
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.CurrentPage) || e.PropertyName == nameof(ViewModel.TotalPages))
            {
                DispatcherQueue.TryEnqueue(() => BuildPaginationButtons());
            }
            if (e.PropertyName == nameof(ViewModel.ActiveTab))
            {
                DispatcherQueue.TryEnqueue(() => UpdateTabStyles());
            }
            if (e.PropertyName == nameof(ViewModel.SelectedOrder))
            {
                DispatcherQueue.TryEnqueue(() => UpdateStatusButtons());
            }
        };
    }

    // ══ Order Selection ═══════════════════════════════════════════
    private async void OrderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView) return;
        if (listView.SelectedItem is not CustomerOrder order) return;

        // SelectOrderAsync loads the order details (items) from the database
        await ViewModel.SelectOrderAsync(order);
        UpdateStatusButtons();
    }

    // ══ Tab Handlers ═══════════════════════════════════════════════
    private void TabAll_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ActiveTab = "All";
    }

    private void TabPending_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ActiveTab = "Pending";
    }

    private void TabProcessing_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ActiveTab = "Processing";
    }

    private void TabDelivered_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ActiveTab = "Delivered";
    }

    private void TabCancelled_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ActiveTab = "Cancelled";
    }

    private void UpdateTabStyles()
    {
        ResetTabStyle(TabAll);
        ResetTabStyle(TabPending);
        ResetTabStyle(TabProcessing);
        ResetTabStyle(TabDelivered);
        ResetTabStyle(TabCancelled);

        var activeTab = ViewModel.ActiveTab switch
        {
            "All" => TabAll,
            "Pending" => TabPending,
            "Processing" => TabProcessing,
            "Delivered" => TabDelivered,
            "Cancelled" => TabCancelled,
            _ => TabAll
        };

        SetTabActiveStyle(activeTab);
    }

    private static readonly Microsoft.UI.Xaml.Media.SolidColorBrush GrayBrush =
        new(Windows.UI.ColorHelper.FromArgb(255, 107, 114, 128));

    private void ResetTabStyle(Button btn)
    {
        btn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Windows.UI.ColorHelper.FromArgb(0, 0, 0, 0));
        btn.Foreground = GrayBrush;
        if (btn.Content is StackPanel sp)
        {
            foreach (var child in sp.Children)
            {
                if (child is TextBlock tb)
                    tb.Foreground = GrayBrush;
                if (child is Border b && b.Child is TextBlock btb)
                    btb.Foreground = GrayBrush;
            }
        }
    }

    private void SetTabActiveStyle(Button btn)
    {
        btn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Windows.UI.ColorHelper.FromArgb(255, 124, 58, 237));
        var whiteBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
            Windows.UI.ColorHelper.FromArgb(255, 255, 255, 255));
        if (btn.Content is StackPanel sp)
        {
            foreach (var child in sp.Children)
            {
                if (child is TextBlock tb)
                    tb.Foreground = whiteBrush;
                if (child is Border b && b.Child is TextBlock btb)
                    btb.Foreground = whiteBrush;
            }
        }
    }

    // ══ Custom Pagination ═════════════════════════════════════════
    private void BuildPaginationButtons()
    {
        PaginationPanel.Children.Clear();

        var current = ViewModel.CurrentPage;
        var total = ViewModel.TotalPages;

        // Prev button
        var prevBtn = new Button
        {
            Content = new TextBlock { Text = "\uE76B", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 12 },
            Style = (Style)Resources["PageBtn"],
            IsEnabled = current > 1,
            Tag = current - 1
        };
        prevBtn.Click += (s, e) =>
        {
            if (s is Button b && b.Tag is int pg)
                ViewModel.GoToPageCommand.Execute(pg);
        };
        PaginationPanel.Children.Add(prevBtn);

        // Page number buttons
        var start = Math.Max(1, current - 2);
        var end = Math.Min(total, current + 2);

        if (start > 1)
        {
            AddPageButton(1, false);
            if (start > 2)
                AddEllipsis();
        }

        for (int i = start; i <= end; i++)
            AddPageButton(i, i == current);

        if (end < total)
        {
            if (end < total - 1)
                AddEllipsis();
            AddPageButton(total, false);
        }

        // Next button
        var nextBtn = new Button
        {
            Content = new TextBlock { Text = "\uE76C", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 12 },
            Style = (Style)Resources["PageBtn"],
            IsEnabled = current < total,
            Tag = current + 1
        };
        nextBtn.Click += (s, e) =>
        {
            if (s is Button b && b.Tag is int pg)
                ViewModel.GoToPageCommand.Execute(pg);
        };
        PaginationPanel.Children.Add(nextBtn);
    }

    private void AddPageButton(int page, bool isActive)
    {
        var btn = new Button
        {
            Content = new TextBlock { Text = page.ToString() },
            Style = (Style)Resources[isActive ? "PageBtnActive" : "PageBtn"],
            Tag = page
        };
        btn.Click += (s, e) =>
        {
            if (s is Button b && b.Tag is int pg)
                ViewModel.GoToPageCommand.Execute(pg);
        };
        PaginationPanel.Children.Add(btn);
    }

    private void AddEllipsis()
    {
        PaginationPanel.Children.Add(new TextBlock
        {
            Text = "...",
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.ColorHelper.FromArgb(255, 156, 163, 175)),
            Margin = new Thickness(4, 0, 4, 0)
        });
    }

    // ══ Detail Panel Buttons ═══════════════════════════════════════
    private void CloseDetail_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CloseDetailPanelCommand.Execute(null);
    }

    private async void StatusPending_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        await ViewModel.UpdateOrderStatusCommand.ExecuteAsync("Pending");
        UpdateStatusButtons();
        SetButtonsEnabled(true);
    }

    private async void StatusProcessing_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        await ViewModel.UpdateOrderStatusCommand.ExecuteAsync("Processing");
        UpdateStatusButtons();
        SetButtonsEnabled(true);
    }

    private async void StatusShipped_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        await ViewModel.UpdateOrderStatusCommand.ExecuteAsync("Shipped");
        UpdateStatusButtons();
        SetButtonsEnabled(true);
    }

    private async void StatusDelivered_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        await ViewModel.UpdateOrderStatusCommand.ExecuteAsync("Delivered");
        UpdateStatusButtons();
        SetButtonsEnabled(true);
    }

    private async void StatusCancelled_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        await ViewModel.UpdateOrderStatusCommand.ExecuteAsync("Cancelled");
        UpdateStatusButtons();
        SetButtonsEnabled(true);
    }

    private async void PayPaid_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        await ViewModel.UpdatePaymentStatusCommand.ExecuteAsync("Paid");
        UpdateStatusButtons();
        SetButtonsEnabled(true);
    }

    private async void PayUnpaid_Click(object sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        await ViewModel.UpdatePaymentStatusCommand.ExecuteAsync("Unpaid");
        UpdateStatusButtons();
        SetButtonsEnabled(true);
    }

    private async void OrderManifest_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedOrder == null || ViewModel.SelectedOrder.Id == 0)
        {
            var errDialog = new ConfirmationDialog(
                "No Order Selected",
                "Please select an order from the list before generating an invoice.");
            errDialog.XamlRoot = XamlRoot;
            await errDialog.ShowAsync();
            return;
        }

        var dialogVm = new InvoiceDialogViewModel(
            ViewModel.SelectedOrder,
            ViewModel.CurrentDetails,
            App.Services.GetRequiredService<IInvoiceService>(),
            App.Services.GetRequiredService<IFilePickerService>(),
            XamlRoot);

        string result = await dialogVm.ExportAsync();

        // Give the UI thread time to fully settle after Export dialog and File Picker
        await Task.Delay(500);

        DispatcherQueue.TryEnqueue(async () =>
        {
            var successDialog = new SuccessDialog("Invoice Export", result);
            successDialog.XamlRoot = XamlRoot;
            await successDialog.ShowAsync();
        });
    }

    private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.DeleteOrderCommand.ExecuteAsync(null);
    }

    // ══ Update Status & Payment Button Highlights ═════════════════
    private void UpdateStatusButtons()
    {
        var orderStatus = ViewModel.SelectedOrder?.Status ?? "";
        SetStatusButtonStyle(BtnStatusPending,    orderStatus, "Pending");
        SetStatusButtonStyle(BtnStatusProcessing, orderStatus, "Processing");
        SetStatusButtonStyle(BtnStatusShipped,   orderStatus, "Shipped");
        SetStatusButtonStyle(BtnStatusDelivered,  orderStatus, "Delivered");
        SetStatusButtonStyle(BtnStatusCancelled,  orderStatus, "Cancelled");

        var payStatus = ViewModel.SelectedOrder?.PaymentStatus ?? "";
        SetStatusButtonStyle(BtnPayPaid,   payStatus, "Paid");
        SetStatusButtonStyle(BtnPayUnpaid, payStatus, "Unpaid");
    }

    private void SetStatusButtonStyle(Button btn, string currentStatus, string btnStatus)
    {
        var isActive = string.Equals(currentStatus, btnStatus, System.StringComparison.OrdinalIgnoreCase);
        btn.Style = (Style)Resources[isActive ? "PrimaryBtn" : "SecondaryBtn"];
    }

    // ══ Status update loading feedback ══════════════════════════════
    private void SetButtonsEnabled(bool enabled)
    {
        BtnStatusPending.IsEnabled = enabled;
        BtnStatusProcessing.IsEnabled = enabled;
        BtnStatusShipped.IsEnabled = enabled;
        BtnStatusDelivered.IsEnabled = enabled;
        BtnStatusCancelled.IsEnabled = enabled;
        BtnPayPaid.IsEnabled = enabled;
        BtnPayUnpaid.IsEnabled = enabled;
    }
}
