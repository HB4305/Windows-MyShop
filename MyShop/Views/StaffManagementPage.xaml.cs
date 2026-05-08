using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Repositories;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class StaffManagementPage : Page
{
    public StaffManagementViewModel ViewModel { get; }

    public StaffManagementPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<StaffManagementViewModel>();
        DataContext = ViewModel;
        Loaded += StaffManagementPage_Loaded;
    }

    private async void StaffManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadStaffCommand.ExecuteAsync(null);
    }

    private async void AddStaff_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddEditStaffDialog
        {
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.CreateStaffCommand.ExecuteAsync(dialog.GetFormData());
        }
    }

    private async void EditStaff_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: UserRecord user })
        {
            return;
        }

        var dialog = new AddEditStaffDialog(user)
        {
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.UpdateStaffCommand.ExecuteAsync(dialog.GetFormData());
        }
    }

    private async void DeleteStaff_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: UserRecord user })
        {
            return;
        }

        var dialog = new ConfirmationDialog(
            "Delete Staff",
            $"Delete staff account '{user.Email}'? This action cannot be undone.")
        {
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteStaffCommand.ExecuteAsync(user);
        }
    }
}
