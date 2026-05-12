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
    private bool _wideSidebarCollapsed;

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
            NavigateToDefaultPage();
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
        if (_compactMode)
        {
            _compactSidebarExpanded = !_compactSidebarExpanded;
        }
        else
        {
            _wideSidebarCollapsed = !_wideSidebarCollapsed;
        }
        ApplyResponsiveLayout(ActualWidth);
    }

    private void ApplyResponsiveLayout(double width)
    {
        var isCompact = width < CompactBreakpoint;
        _compactMode = isCompact;

        if (isCompact)
        {
            HeaderTitleText.FontSize = 18;
            NavToggleButton_Header.Visibility = _compactSidebarExpanded ? Visibility.Collapsed : Visibility.Visible;
            NavToggleButton_Sidebar.Visibility = Visibility.Visible;
            SidebarColumn.Width = _compactSidebarExpanded ? new GridLength(220) : new GridLength(0);
            SidebarPanel.Visibility = _compactSidebarExpanded ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        // Wide mode
        HeaderTitleText.FontSize = 22;
        if (_wideSidebarCollapsed)
        {
            NavToggleButton_Header.Visibility = Visibility.Visible;
            NavToggleButton_Sidebar.Visibility = Visibility.Collapsed;
            SidebarColumn.Width = new GridLength(0);
            SidebarPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            NavToggleButton_Header.Visibility = Visibility.Collapsed;
            NavToggleButton_Sidebar.Visibility = Visibility.Visible;
            SidebarColumn.Width = new GridLength(240);
            SidebarPanel.Visibility = Visibility.Visible;
        }
    }

    private void MaintainSidebarAfterNavigation()
    {
        if (!_compactMode)
            return;

        ApplyResponsiveLayout(ActualWidth);
    }

    /// <summary>
    /// Shows/hides menu items based on the current user's role.
    /// - Owner: manages the shop and reviews shift logs
    /// - Sale: works from POS and submits shift reports
    /// </summary>
    private void ApplyRolePermissions()
    {
        var isOwner = _currentUserService.IsOwner;

        NavDashboard.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavReports.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavAiStudio.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavPos.Visibility = _currentUserService.IsSale ? Visibility.Visible : Visibility.Collapsed;
        NavShiftManagement.Visibility = Visibility.Visible;
        NavProductCatalog.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavOrders.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavCustomers.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavStaffManagement.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavSuppliers.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavCategory.Visibility = isOwner ? Visibility.Visible : Visibility.Collapsed;
        NavSettings.Visibility = Visibility.Visible;
        HeaderSettingsButton.Visibility = Visibility.Visible;

        NavDashboard.IsEnabled = isOwner;
        NavReports.IsEnabled = isOwner;
        NavAiStudio.IsEnabled = isOwner;
        NavPos.IsEnabled = _currentUserService.IsSale;
        NavProductCatalog.IsEnabled = isOwner;
        NavOrders.IsEnabled = isOwner;
        NavCustomers.IsEnabled = isOwner;
        NavStaffManagement.IsEnabled = isOwner;
        NavSuppliers.IsEnabled = isOwner;
        NavCategory.IsEnabled = isOwner;
        NavSettings.IsEnabled = true;
        HeaderSettingsButton.IsEnabled = true;

        ShiftNavigationText.Text = isOwner ? "Shift Log" : "Shift Report";
        NavShiftManagement.Tag = isOwner ? "ShiftLogs" : "ShiftReport";
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

    private void NavPos_Click(object sender, RoutedEventArgs e)
    {
        _frame.Navigate(typeof(PosPage));
        UpdateActiveNav("POS");
        MaintainSidebarAfterNavigation();
    }

    private void NavShiftManagement_Click(object sender, RoutedEventArgs e)
    {
        if (_currentUserService.IsOwner)
        {
            _frame.Navigate(typeof(ShiftReportLogsPage));
            UpdateActiveNav("ShiftLogs");
        }
        else
        {
            _frame.Navigate(typeof(ShiftManagementPage));
            UpdateActiveNav("ShiftReport");
        }

        MaintainSidebarAfterNavigation();
    }

    private void NavReports_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(ReportPage));
        UpdateActiveNav("Reports");
        MaintainSidebarAfterNavigation();
    }

    private void NavAiStudio_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(AiStudioPage));
        UpdateActiveNav("AiStudio");
        MaintainSidebarAfterNavigation();
    }

    private void NavProductCatalog_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(SportItemPage));
        UpdateActiveNav("ProductCatalog");
        MaintainSidebarAfterNavigation();
    }

    private void NavOrders_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(CustomerOrderPage));
        UpdateActiveNav("OrdersManagement");
        MaintainSidebarAfterNavigation();
    }

    private void NavCustomers_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(CustomerPage));
        UpdateActiveNav("Customers");
        MaintainSidebarAfterNavigation();
    }

    private void NavStaffManagement_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.IsOwner) return;
        _frame.Navigate(typeof(StaffManagementPage));
        UpdateActiveNav("StaffManagement");
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

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new MyShop.Views.Dialogs.ConfirmationDialog(
            "Sign Out?",
            "Do you want to sign out from MyShop?",
            confirmText: "Sign Out",
            glyph: "\uE7E8")
        {
            XamlRoot = this.XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            ShellPageEvents.RaiseLogout();
        }
    }

    private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (e.SourcePageType == null) return;
        
        string? tag = null;
        string title = "Welcome back";

        switch (e.SourcePageType.Name)
        {
            case nameof(DashboardPage):
                tag = "Dashboard";
                title = "Dashboard Overview";
                break;
            case nameof(PosPage):
                tag = "POS";
                title = "Point of Sale";
                break;
            case nameof(ShiftManagementPage):
                tag = "ShiftReport";
                title = "Submit Shift Report";
                break;
            case nameof(ShiftReportLogsPage):
                tag = "ShiftLogs";
                title = "Shift History & Logs";
                break;
            case nameof(ReportPage):
                tag = "Reports";
                title = "Business Analytics & Reports";
                break;
            case nameof(AiStudioPage):
                tag = "AiStudio";
                title = "AI Studio";
                break;
            case nameof(SportItemPage):
            case nameof(ProductCatalogPage):
                tag = "ProductCatalog";
                title = "Product Catalog";
                break;
            case nameof(CustomerOrderPage):
            case nameof(OrdersManagementPage):
                tag = "OrdersManagement";
                title = "Orders Management";
                break;
            case nameof(CustomerPage):
                tag = "Customers";
                title = "Customer Directory";
                break;
            case nameof(StaffManagementPage):
                tag = "StaffManagement";
                title = "Staff Management";
                break;
            case nameof(SuppliersPage):
                tag = "Suppliers";
                title = "Supplier Management";
                break;
            case nameof(CategoryPage):
                tag = "Category";
                title = "Category Management";
                break;
            case nameof(SettingsPage):
                tag = "Settings";
                title = "Application Settings";
                break;
        }

        if (tag != null)
        {
            UpdateActiveNav(tag);
            HeaderTitleText.Text = title;
            _settingsManager.SetLastActivity(tag);
        }
    }

    private void UpdateActiveNav(string activeTag)
    {
        ResetNavStyle(NavDashboard);
        ResetNavStyle(NavPos);
        ResetNavStyle(NavShiftManagement);
        ResetNavStyle(NavReports);
        ResetNavStyle(NavAiStudio);
        ResetNavStyle(NavProductCatalog);
        ResetNavStyle(NavOrders);
        ResetNavStyle(NavCustomers);
        ResetNavStyle(NavStaffManagement);
        ResetNavStyle(NavSuppliers);
        ResetNavStyle(NavCategory);
        ResetNavStyle(NavSettings);

        var activeBtn = activeTag switch
        {
            "Dashboard" => NavDashboard,
            "POS" => NavPos,
            "ShiftReport" => NavShiftManagement,
            "ShiftLogs" => NavShiftManagement,
            "Reports" => NavReports,
            "AiStudio" => NavAiStudio,
            "ProductCatalog" => NavProductCatalog,
            "OrdersManagement" => NavOrders,
            "Customers" => NavCustomers,
            "StaffManagement" => NavStaffManagement,
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

        tag = NormalizeActivityTag(tag);

        if (!CanNavigateToTag(tag))
        {
            return false;
        }

        var pageType = tag switch
        {
            "Dashboard" => typeof(DashboardPage),
            "POS" => typeof(PosPage),
            "ShiftReport" => typeof(ShiftManagementPage),
            "ShiftLogs" => typeof(ShiftReportLogsPage),
            "Reports" => typeof(ReportPage),
            "AiStudio" => typeof(AiStudioPage),
            "ProductCatalog" => typeof(SportItemPage),
            "OrdersManagement" => typeof(CustomerOrderPage),
            "Customers" => typeof(CustomerPage),
            "StaffManagement" => typeof(StaffManagementPage),
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

    private void NavigateToDefaultPage()
    {
        if (_currentUserService.IsSale)
        {
            _frame.Navigate(typeof(PosPage));
            UpdateActiveNav("POS");
            return;
        }

        _frame.Navigate(typeof(DashboardPage));
        UpdateActiveNav("Dashboard");
    }

    private bool CanNavigateToTag(string tag)
    {
        if (_currentUserService.IsSale)
        {
            return tag is "POS" or "ShiftReport" or "Settings";
        }

        if (_currentUserService.IsOwner)
        {
            return tag is "Dashboard"
                or "POS"
                or "ShiftLogs"
                or "Reports"
                or "AiStudio"
                or "ProductCatalog"
                or "OrdersManagement"
                or "Customers"
                or "StaffManagement"
                or "Suppliers"
                or "Category"
                or "Settings";
        }

        return false;
    }

    private string NormalizeActivityTag(string tag)
    {
        if (tag == "ShiftManagement")
        {
            return _currentUserService.IsOwner ? "ShiftLogs" : "ShiftReport";
        }

        return tag;
    }

    private void ResetNavStyle(Button btn)
    {
        btn.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        if (btn.Content is not StackPanel stack) return;
        
        var secondaryBrush = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        
        foreach (var child in stack.Children)
        {
            if (child is TextBlock tb)
            {
                tb.Foreground = secondaryBrush;
                tb.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
            }
        }
    }

    private void SetActiveNavStyle(Button btn)
    {
        btn.Background = (Brush)Application.Current.Resources["PurpleLightBrush"];
        if (btn.Content is not StackPanel stack) return;
        
        var activeBrush = (Brush)Application.Current.Resources["PurpleBrush"];
        
        foreach (var child in stack.Children)
        {
            if (child is TextBlock tb)
            {
                tb.Foreground = activeBrush;
                tb.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
            }
        }
    }
}
