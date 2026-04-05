using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class ConfigPage : Page
{
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
        ConfigPasswordTextBox.TextChanged += OnConfigPasswordTextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ConfigViewModel vm)
        {
            DbPasswordBox.Password = vm.DbPassword;
            ConfigPasswordTextBox.Text = vm.DbPassword;
        }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ConfigViewModel vm)
            vm.DbPassword = DbPasswordBox.Password;
    }

    private void OnConfigPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is ConfigViewModel vm)
            vm.DbPassword = ConfigPasswordTextBox.Text;
    }

    private bool _isConfigPasswordVisible = false;

    private void OnToggleConfigPasswordClick(object sender, RoutedEventArgs e)
    {
        if (_isConfigPasswordVisible)
        {
            DbPasswordBox.Password = ConfigPasswordTextBox.Text;
            DbPasswordBox.Visibility = Visibility.Visible;
            ConfigPasswordTextBox.Visibility = Visibility.Collapsed;
            ToggleConfigPasswordIcon.Text = "\uE7B3";
            ToolTipService.SetToolTip(ToggleConfigPasswordButton, "Show password");
            _isConfigPasswordVisible = false;
        }
        else
        {
            ConfigPasswordTextBox.Text = DbPasswordBox.Password;
            DbPasswordBox.Visibility = Visibility.Collapsed;
            ConfigPasswordTextBox.Visibility = Visibility.Visible;
            ToggleConfigPasswordIcon.Text = "\uE7B4";
            ToolTipService.SetToolTip(ToggleConfigPasswordButton, "Hide password");
            _isConfigPasswordVisible = true;
        }
    }
}
