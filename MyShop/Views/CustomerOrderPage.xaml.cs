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
    private const double SingleColumnBreakpoint = 1100;
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
                ApplyResponsiveLayout(ActualWidth);
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

    private void OrderRoot_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout(e.NewSize.Width);
    }

    private void ApplyResponsiveLayout(double width)
    {
        if (width < SingleColumnBreakpoint)
        {
            SetSingleColumnLayout();
            return;
        }

        SetTwoColumnLayout();
    }

    private void SetTwoColumnLayout()
    {
        OrderRoot.ColumnDefinitions.Clear();
        OrderRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        OrderRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(420) });

        OrderRoot.RowDefinitions.Clear();
        OrderRoot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        OrderRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid.SetRow(OrderListPanel, 0);
        Grid.SetColumn(OrderListPanel, 0);
        Grid.SetColumnSpan(OrderListPanel, 1);

        Grid.SetRow(OrderDetailPanel, 0);
        Grid.SetColumn(OrderDetailPanel, 1);
        Grid.SetColumnSpan(OrderDetailPanel, 1);

        Grid.SetRow(OrderEmptyDetailPanel, 0);
        Grid.SetColumn(OrderEmptyDetailPanel, 1);
        Grid.SetColumnSpan(OrderEmptyDetailPanel, 1);
    }

    private void SetSingleColumnLayout()
    {
        OrderRoot.ColumnDefinitions.Clear();
        OrderRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        OrderRoot.RowDefinitions.Clear();
        OrderRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        OrderRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid.SetRow(OrderListPanel, 0);
        Grid.SetColumn(OrderListPanel, 0);
        Grid.SetColumnSpan(OrderListPanel, 1);

        Grid.SetRow(OrderDetailPanel, 1);
        Grid.SetColumn(OrderDetailPanel, 0);
        Grid.SetColumnSpan(OrderDetailPanel, 1);

        Grid.SetRow(OrderEmptyDetailPanel, 1);
        Grid.SetColumn(OrderEmptyDetailPanel, 0);
        Grid.SetColumnSpan(OrderEmptyDetailPanel, 1);
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
        if (PaginationPanel == null) return;
        PaginationPanel.Children.Clear();

        var total = ViewModel.TotalPages;
        var current = ViewModel.CurrentPage;

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
                btn.Style = (Style)Resources["PageBtnActive"];
            }
            else
            {
                btn.Style = (Style)Resources["PageBtn"];
            }

            btn.Click += (s, e) =>
            {
                ViewModel.GoToPageCommand.Execute(page);
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
            Style = (Style)Resources["PageBtn"],
        };
        prevBtn.Click += (s, e) => { if (current > 1) { ViewModel.GoToPageCommand.Execute(current - 1); } };
        PaginationPanel.Children.Add(prevBtn);

        // Page numbers logic (smart ellipsis)
        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
                AddPageBtn(i, i == current);
        }
        else
        {
            AddPageBtn(1, current == 1);
            if (current > 3) PaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextFillColorSecondaryBrush"] });

            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
                AddPageBtn(i, i == current);

            if (current < total - 2) PaginationPanel.Children.Add(new TextBlock { Text = "...", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0), Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextFillColorSecondaryBrush"] });
            AddPageBtn(total, current == total);
        }

        // Next button
        var nextBtn = new Button
        {
            Content = new TextBlock { Text = "Next", FontSize = 13 },
            MinWidth = 52,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Style = (Style)Resources["PageBtn"],
        };
        nextBtn.Click += (s, e) => { if (current < total) { ViewModel.GoToPageCommand.Execute(current + 1); } };
        PaginationPanel.Children.Add(nextBtn);
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

        if (string.IsNullOrEmpty(result)) return;
        
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
