using System;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WpfFontWeights = Microsoft.UI.Text.FontWeights;

namespace MyShop.Views;

public sealed partial class ShellPage : Page
{
    private readonly Frame _frame;

    public ShellPage()
    {
        this.InitializeComponent();
        _frame = ContentFrame;
        _frame.Navigate(typeof(DashboardPage));
        UpdateActiveNav("Dashboard");
    }

    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(DashboardPage));
        UpdateActiveNav("Dashboard");
    }

    private void NavReports_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(ReportPage));
        UpdateActiveNav("Reports");
    }

    private void NavProductCatalog_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(SportItemPage));
        UpdateActiveNav("ProductCatalog");
    }

    private void NavOrders_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(OrdersManagementPage));
        UpdateActiveNav("OrdersManagement");
    }

    private void NavSalesStats_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(SalesStatsPage));
        UpdateActiveNav("SalesStats");
    }

    private void NavSuppliers_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(SuppliersPage));
        UpdateActiveNav("Suppliers");
    }

    private void NavCategory_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(CategoryPage));
        UpdateActiveNav("Category");
    }

    private void NavSettings_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(SettingsPage));
        UpdateActiveNav("Settings");
    }

    private void NavNotifications_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Show notifications panel
    }

    private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (e.SourcePageType == null) return;
        var tag = e.SourcePageType.Name switch
        {
            nameof(DashboardPage) => "Dashboard",
            nameof(ReportPage) => "Reports",
            nameof(SportItemPage) => "ProductCatalog",
            nameof(ProductCatalogPage) => "ProductCatalog",
            nameof(OrdersManagementPage) => "OrdersManagement",
            nameof(SalesStatsPage) => "SalesStats",
            nameof(SuppliersPage) => "Suppliers",
            nameof(CategoryPage) => "Category",
            nameof(SettingsPage) => "Settings",
            _ => null
        };
        if (tag != null) UpdateActiveNav(tag);
    }

    private void UpdateActiveNav(string activeTag)
    {
        ResetNavStyle(NavDashboard);
        ResetNavStyle(NavReports);
        ResetNavStyle(NavProductCatalog);
        ResetNavStyle(NavOrders);
        ResetNavStyle(NavSalesStats);
        ResetNavStyle(NavSuppliers);
        ResetNavStyle(NavCategory);
        ResetNavStyle(NavSettings);

        var activeBtn = activeTag switch
        {
            "Dashboard" => NavDashboard,
            "Reports" => NavReports,
            "ProductCatalog" => NavProductCatalog,
            "OrdersManagement" => NavOrders,
            "SalesStats" => NavSalesStats,
            "Suppliers" => NavSuppliers,
            "Category" => NavCategory,
            "Settings" => NavSettings,
            _ => null
        };
        if (activeBtn != null) SetActiveNavStyle(activeBtn);
    }

    private void ResetNavStyle(Button btn)
    {
        btn.Background = new SolidColorBrush(new Windows.UI.Color() { A = 0, R = 0, G = 0, B = 0 });
        if (btn.Content is not StackPanel stack) return;
        foreach (var child in stack.Children)
        {
            if (child is TextBlock tb)
            {
                tb.Foreground = new SolidColorBrush(Color.FromArgb(255, 107, 114, 128));
                tb.FontWeight = WpfFontWeights.Normal;
            }
        }
    }

    private void SetActiveNavStyle(Button btn)
    {
        btn.Background = new SolidColorBrush(Color.FromArgb(255, 243, 240, 255));
        if (btn.Content is not StackPanel stack) return;
        foreach (var child in stack.Children)
        {
            if (child is TextBlock tb)
            {
                tb.Foreground = new SolidColorBrush(Color.FromArgb(255, 124, 58, 237));
                tb.FontWeight = WpfFontWeights.SemiBold;
            }
        }
    }
}
