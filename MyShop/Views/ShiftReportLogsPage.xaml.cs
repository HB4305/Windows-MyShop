using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Services;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class ShiftReportLogsPage : Page
{
    public ShiftReportLogsViewModel ViewModel { get; }

    public ShiftReportLogsPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ShiftReportLogsViewModel>();
        DataContext = ViewModel;
        Loaded += ShiftReportLogsPage_Loaded;
    }

    private async void ShiftReportLogsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadLogsCommand.ExecuteAsync(null);
    }

    private void BackToShiftManagement_Click(object sender, RoutedEventArgs e)
    {
        var currentUser = App.Services.GetRequiredService<CurrentUserService>();
        Frame.Navigate(currentUser.IsOwner ? typeof(DashboardPage) : typeof(ShiftManagementPage));
    }
}
