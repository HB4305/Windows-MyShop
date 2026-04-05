using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class LoginPage : Page
{
    public static string AppVersion => LoginViewModel.AppVersion;

    public LoginPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<LoginViewModel>();

        Loaded += OnLoaded;
        PasswordBox.PasswordChanged += OnPasswordChanged;
        PasswordTextBox.TextChanged += OnPasswordTextBoxTextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            PasswordBox.Password = vm.Password;
            PasswordTextBox.Text = vm.Password;
        }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }

    private void OnPasswordTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordTextBox.Text;
    }

    private bool _isPasswordVisible = false;

    private void OnTogglePasswordClick(object sender, RoutedEventArgs e)
    {
        if (_isPasswordVisible)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            TogglePasswordIcon.Text = "\uE7B3";
            ToolTipService.SetToolTip(TogglePasswordButton, "Show password");
            _isPasswordVisible = false;
        }
        else
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
            TogglePasswordIcon.Text = "\uE7B4";
            ToolTipService.SetToolTip(TogglePasswordButton, "Hide password");
            _isPasswordVisible = true;
        }
    }
}
