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
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Reset form — tránh PasswordBox giữ giá trị cũ khi reused từ backstack
        // Email giữ nguyên (CredentialManager vẫn lưu SavedEmail sau logout)
        PasswordBox.Password = string.Empty;

        if (DataContext is LoginViewModel vm)
        {
            vm.Password = string.Empty;
            vm.ErrorMessage = string.Empty;
        }

        _ = TryAutoLoginAsync();
    }

    private async Task TryAutoLoginAsync()
    {
        if (DataContext is not LoginViewModel vm)
            return;

        // Nếu auto-login thành công, vm.Password đã được set
        // → RaiseLoginSuccess → App.xaml.cs NavigateToMain → thoát khỏi trang này
        // Nếu thất bại → form vẫn hiển thị với email đã điền sẵn, user tự nhập pass
        await vm.TryAutoLoginAsync();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }

    private void TogglePassword_Click(object sender, RoutedEventArgs e)
    {
        PasswordBox.PasswordRevealMode = PasswordBox.PasswordRevealMode == PasswordRevealMode.Visible
            ? PasswordRevealMode.Hidden
            : PasswordRevealMode.Visible;
    }
}
