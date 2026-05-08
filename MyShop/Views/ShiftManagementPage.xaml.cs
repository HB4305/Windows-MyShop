using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class ShiftManagementPage : Page
{
    public ShiftManagementPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ShiftViewModel>();
        Loaded += ShiftManagementPage_Loaded;
    }

    private ShiftViewModel? ViewModel => DataContext as ShiftViewModel;

    private async void ShiftManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null)
        {
            await ViewModel.LoadActiveShiftCommand.ExecuteAsync(null);
        }
    }
}
