using System;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.Services;
using WpfFontWeights = Microsoft.UI.Text.FontWeights;

namespace MyShop.Views;

public sealed partial class ShellPage : Page
{
    private readonly Frame _frame;
    private readonly SettingsManager _settingsManager;

    public ShellPage()
    {
        this.InitializeComponent();
        _settingsManager = App.Services.GetRequiredService<SettingsManager>();
        _frame = ContentFrame;
        var remember = _settingsManager.GetRememberLastActivity();
        var lastActivity = remember ? _settingsManager.GetLastActivity() : null;

        if (!TryNavigateToTag(lastActivity))
        {
            _frame.Navigate(typeof(DashboardPage));
            UpdateActiveNav("Dashboard");
        }
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
        _frame.Navigate(typeof(CustomerOrderPage));
        UpdateActiveNav("OrdersManagement");
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
            nameof(CustomerOrderPage) => "OrdersManagement",
            nameof(OrdersManagementPage) => "OrdersManagement",

            nameof(CategoryPage) => "Category",
            nameof(SettingsPage) => "Settings",
            _ => null
        };
        if (tag != null)
        {
            UpdateActiveNav(tag);
            if (_settingsManager.GetRememberLastActivity())
                _settingsManager.SetLastActivity(tag);
        }
    }

    private void UpdateActiveNav(string activeTag)
    {
        ResetNavStyle(NavDashboard);
        ResetNavStyle(NavReports);
        ResetNavStyle(NavProductCatalog);
        ResetNavStyle(NavOrders);

        ResetNavStyle(NavCategory);
        ResetNavStyle(NavSettings);

        var activeBtn = activeTag switch
        {
            "Dashboard" => NavDashboard,
            "Reports" => NavReports,
            "ProductCatalog" => NavProductCatalog,
            "OrdersManagement" => NavOrders,

            "Category" => NavCategory,
            "Settings" => NavSettings,
            _ => null
        };
        if (activeBtn != null) SetActiveNavStyle(activeBtn);
    }

    private bool TryNavigateToTag(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        var pageType = tag switch
        {
            "Dashboard" => typeof(DashboardPage),
            "Reports" => typeof(ReportPage),
            "ProductCatalog" => typeof(SportItemPage),
            "OrdersManagement" => typeof(CustomerOrderPage),
            "Category" => typeof(CategoryPage),
            "Settings" => typeof(SettingsPage),
            _ => null
        };

        if (pageType == null)
            return false;

        _frame.Navigate(pageType);
        UpdateActiveNav(tag);
        return true;
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
