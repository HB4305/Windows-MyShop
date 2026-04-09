using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class LoginPage : Page
{
    public static string AppVersion => LoginViewModel.AppVersion;

    private LoginViewModel? _vm;

    public LoginPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<LoginViewModel>();
        _vm = DataContext as LoginViewModel;

        Loaded += OnLoaded;
        PasswordBox.PasswordChanged += OnPasswordChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PasswordBox.Password = string.Empty;

        if (_vm != null)
        {
            _vm.Password = string.Empty;
            _vm.ErrorMessage = string.Empty;
        }

        _ = TryAutoLoginAsync();
    }

    private async Task TryAutoLoginAsync()
    {
        if (_vm == null) return;
        await _vm.TryAutoLoginAsync();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_vm != null)
            _vm.Password = PasswordBox.Password;
    }

    private void TogglePassword_Click(object sender, RoutedEventArgs e)
    {
        PasswordBox.PasswordRevealMode = PasswordBox.PasswordRevealMode == PasswordRevealMode.Visible
            ? PasswordRevealMode.Hidden
            : PasswordRevealMode.Visible;
    }

    private async void PrimaryActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_vm == null) return;
        _vm.Password = PasswordBox.Password;
        await _vm.LoginCommand.ExecuteAsync(null);
    }
}
