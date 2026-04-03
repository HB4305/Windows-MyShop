using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class ConfigPage : Page
{
    private bool _passwordVisible;

    /// <summary>
    /// Exposes AppVersion as an instance property for XAML binding.
    /// </summary>
    public string AppVersion => ConfigViewModel.AppVersion;

    public ConfigPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<ConfigViewModel>();

        Loaded += OnLoaded;
        DbPasswordBox.PasswordChanged += OnPasswordChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ConfigViewModel vm)
            DbPasswordBox.Password = vm.DbPassword;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ConfigViewModel vm)
            vm.DbPassword = DbPasswordBox.Password;
    }

    private void TogglePassword_Click(object sender, RoutedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        DbPasswordBox.PasswordRevealMode = _passwordVisible
            ? PasswordRevealMode.Visible
            : PasswordRevealMode.Hidden;
    }
}
