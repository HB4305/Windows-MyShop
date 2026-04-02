using MyShop.Models;
using MyShop.ViewModels;
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
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, BuildPaginationButtons);
            }
            if (e.PropertyName == nameof(ViewModel.ActiveTab))
            {
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, UpdateTabStyles);
            }
        };
    }

    // ══ Order Selection ═══════════════════════════════════════════
    private async void OrderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView) return;
        if (listView.SelectedItem is not CustomerOrder order) return;

        try
        {
            await ViewModel.SelectOrderAsync(order);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CustomerOrderPage] SelectOrder failed: {ex.Message}");
        }
        finally
        {
            listView.SelectedItem = null;
        }
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

    private void TabCompleted_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ActiveTab = "Completed";
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
        ResetTabStyle(TabCompleted);
        ResetTabStyle(TabCancelled);

        var activeTab = ViewModel.ActiveTab switch
        {
            "All" => TabAll,
            "Pending" => TabPending,
            "Processing" => TabProcessing,
            "Completed" => TabCompleted,
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

    private void StatusPending_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateOrderStatusCommand.Execute("Pending");
    }

    private void StatusProcessing_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateOrderStatusCommand.Execute("Processing");
    }

    private void StatusShipped_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateOrderStatusCommand.Execute("Shipped");
    }

    private void StatusDelivered_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateOrderStatusCommand.Execute("Delivered");
    }

    private void StatusCancelled_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateOrderStatusCommand.Execute("Cancelled");
    }

    private void PayPaid_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdatePaymentStatusCommand.Execute("Paid");
    }

    private void PayUnpaid_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdatePaymentStatusCommand.Execute("Unpaid");
    }

    private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.DeleteOrderCommand.ExecuteAsync(null);
    }
}
