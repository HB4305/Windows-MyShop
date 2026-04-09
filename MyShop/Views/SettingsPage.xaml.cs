using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;
using MyShop.Views.Dialogs;

namespace MyShop.Views;

public sealed partial class SettingsPage : Page
{
    private SettingsViewModel? _vm;

    public SettingsPage()
    {
        this.InitializeComponent();
        _vm = App.Services.GetRequiredService<SettingsViewModel>();
        DataContext = _vm;
    }

    private async void SaveChanges_Click(object sender, RoutedEventArgs e)
    {
        if (_vm == null) return;
        _vm.SaveChangesCommand.Execute(null);
        var dialog = new SuccessDialog("Success", "Settings saved.");
        dialog.XamlRoot = XamlRoot;
        await dialog.ShowAsync();
    }

    private void NewUserPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_vm != null && sender is PasswordBox pb)
            _vm.NewUserPassword = pb.Password;
    }
}
