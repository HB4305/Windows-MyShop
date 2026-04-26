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
    private const double CompactBreakpoint = 1100;
    private readonly Frame _frame;
    private readonly SettingsManager _settingsManager;
    private readonly CurrentUserService _currentUserService;
    private bool _compactMode;
    private bool _compactSidebarExpanded;

    public ShellPage()
    {
        this.InitializeComponent();
        _settingsManager = App.Services.GetRequiredService<SettingsManager>();
        _currentUserService = App.Services.GetRequiredService<CurrentUserService>();
        _frame = ContentFrame;

        Loaded += ShellPage_OnLoaded;
        SizeChanged += ShellPage_SizeChanged;

        var remember = _settingsManager.GetRememberLastActivity();
        var lastActivity = remember ? _settingsManager.GetLastActivity() : null;

        if (!TryNavigateToTag(lastActivity))
        {
            _frame.Navigate(typeof(DashboardPage));
            UpdateActiveNav("Dashboard");
        }
    }

    private void ShellPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyRolePermissions();
        UpdateUserCard();
        ApplyResponsiveLayout(ActualWidth);
    }

    private void ShellPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout(e.NewSize.Width);
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        if (!_compactMode)
            return;

        _compactSidebarExpanded = !_compactSidebarExpanded;
        ApplyResponsiveLayout(ActualWidth);
    }

    private void ApplyResponsiveLayout(double width)
    {
        var isCompact = width < CompactBreakpoint;
        _compactMode = isCompact;

        if (isCompact)
        {
            NavToggleButton.Visibility = Visibility.Visible;
            HeaderTitleText.Text = "Welcome back";
            HeaderTitleText.FontSize = 18;

            SidebarColumn.Width = _compactSidebarExpanded ? new GridLength(220) : new GridLength(0);
            SidebarPanel.Visibility = _compactSidebarExpanded ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        NavToggleButton.Visibility = Visibility.Collapsed;
        HeaderTitleText.Text = "Welcome back, Alex";
        HeaderTitleText.FontSize = 22;
        SidebarColumn.Width = new GridLength(240);
        SidebarPanel.Visibility = Visibility.Visible;
    }

    private void MaintainSidebarAfterNavigation()
    {
        if (!_compactMode)
            return;

        ApplyResponsiveLayout(ActualWidth);
    }

    /// <summary>
    /// Shows/hides menu items based on the current user's role.
    /// - Owner: sees all menus
    /// - Sale: hides Reports, Category
    /// </summary>
    private void ApplyRolePermissions()
    {
        var isOwner = _currentUserService.IsOwner;

        // Hide Reports, Category and Suppliers for sales roles
        NavReports.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavCategory.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavSuppliers.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;

        // Sales are not allowed to access Reports/Category/Suppliers
        NavReports.IsEnabled = isOwner;
        NavCategory.IsEnabled = isOwner;
        NavSuppliers.IsEnabled = isOwner;
    }

    /// <summary>
    /// Updates user information in the card (email + role) from the session.
    /// </summary>
    private void UpdateUserCard()
    {
        var email = _currentUserService.UserEmail ?? "User";
        ProfileNameText.Text = email;
        ProfileRoleText.Text = _currentUserService.UserRole?.ToUpperInvariant() ?? "";
        ProfileAvatarText.Text = GetInitials(email);
    }

    private static string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return (parts[0][0] + "" + parts[1][0]).ToUpperInvariant();
        return parts.Length > 0 ? parts[0][0].ToString().ToUpperInvariant() : "U";
    }

    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(DashboardPage));
        UpdateActiveNav("Dashboard");
        MaintainSidebarAfterNavigation();
    }

    private void NavReports_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(ReportPage));
        UpdateActiveNav("Reports");
        MaintainSidebarAfterNavigation();
    }

    private void NavProductCatalog_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(SportItemPage));
        UpdateActiveNav("ProductCatalog");
        MaintainSidebarAfterNavigation();
    }

    private void NavOrders_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(CustomerOrderPage));
        UpdateActiveNav("OrdersManagement");
        MaintainSidebarAfterNavigation();
    }

    private void NavCustomers_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(CustomerPage));
        UpdateActiveNav("Customers");
        MaintainSidebarAfterNavigation();
    }

    private void NavSuppliers_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(SuppliersPage));
        UpdateActiveNav("Suppliers");
        MaintainSidebarAfterNavigation();
    }

    private void NavCategory_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(CategoryPage));
        UpdateActiveNav("Category");
        MaintainSidebarAfterNavigation();
    }

    private void NavSettings_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(SettingsPage));
        UpdateActiveNav("Settings");
        MaintainSidebarAfterNavigation();
    }

    private void NavNotifications_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Show notifications panel
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
        => ShellPageEvents.RaiseLogout();

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
            nameof(CustomerPage) => "Customers",
            nameof(SuppliersPage) => "Suppliers",
            nameof(CategoryPage) => "Category",
            nameof(SettingsPage) => "Settings",
            _ => null
        };
        if (tag != null)
        {
            UpdateActiveNav(tag);
            // Always save the current page as last activity — the settings toggle
            // only controls whether it is RESTORED on next startup, not whether it is saved.
            _settingsManager.SetLastActivity(tag);
        }
    }

    private void UpdateActiveNav(string activeTag)
    {
        ResetNavStyle(NavDashboard);
        ResetNavStyle(NavReports);
        ResetNavStyle(NavProductCatalog);
        ResetNavStyle(NavOrders);
        ResetNavStyle(NavCustomers);
        ResetNavStyle(NavSuppliers);
        ResetNavStyle(NavCategory);
        ResetNavStyle(NavSettings);

        var activeBtn = activeTag switch
        {
            "Dashboard" => NavDashboard,
            "Reports" => NavReports,
            "ProductCatalog" => NavProductCatalog,
            "OrdersManagement" => NavOrders,
            "Customers" => NavCustomers,
            "Suppliers" => NavSuppliers,
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

        // Sales cannot navigate to forbidden pages
        if (_currentUserService.IsSale &&
            (tag == "Reports" || tag == "Category"))
            return false;

        var pageType = tag switch
        {
            "Dashboard" => typeof(DashboardPage),
            "Reports" => typeof(ReportPage),
            "ProductCatalog" => typeof(SportItemPage),
            "OrdersManagement" => typeof(CustomerOrderPage),
            "Customers" => typeof(CustomerPage),
            "Suppliers" => typeof(SuppliersPage),
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
