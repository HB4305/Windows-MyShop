using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class LoginPage : Page
{
    private bool _passwordVisible;

    public LoginPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<LoginViewModel>();

        Loaded += OnLoaded;
        PasswordBox.PasswordChanged += OnPasswordChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            PasswordBox.Password = vm.Password;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }

    private void TogglePassword_Click(object sender, RoutedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;

        if (_passwordVisible)
        {
            // Show password in TextBox overlay
            PasswordBox.PasswordRevealMode = PasswordRevealMode.Visible;
        }
        else
        {
            PasswordBox.PasswordRevealMode = PasswordRevealMode.Hidden;
        }
    }
}
