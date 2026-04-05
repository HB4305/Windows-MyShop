using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Repositories;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// Sự kiện điều hướng từ LoginPage.
/// </summary>
public static class LoginPageEvents
{
    public static event Action? OnLoginSuccess;
    public static event Action? OnNavigateToConfig;
    public static event Action? OnNavigateToLogin;
    public static void RaiseLoginSuccess() => OnLoginSuccess?.Invoke();
    public static void RaiseNavigateToConfig() => OnNavigateToConfig?.Invoke();
    public static void RaiseNavigateToSignUp() => OnNavigateToSignUp?.Invoke();
    public static void RaiseNavigateToLogin() => OnNavigateToLogin?.Invoke();
}

public partial class LoginViewModel : ObservableObject
{
    private readonly CredentialManager _credentialManager;
    private readonly UserRepository _userRepository;

    public LoginViewModel(CredentialManager credentialManager, UserRepository userRepository)
    {
        _credentialManager = credentialManager;
        _userRepository = userRepository;
        Email = _credentialManager.GetSavedEmail() ?? string.Empty;
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Phiên bản ứng dụng hiển thị ở màn hình login.
    /// </summary>
    public static string AppVersion
    {
        get
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            var verStr = ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
            return $"Version {verStr} · © {DateTime.Now.Year} ProSport";
        }
    }

    /// <summary>
    /// Đăng nhập bằng credentials đã lưu (auto-login).
    /// Thử khôi phục session từ credentials đã lưu.
    /// </summary>
    public async Task<bool> TryAutoLoginAsync()
    {
        if (!_credentialManager.HasSavedCredentials())
            return false;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var savedEmail = _credentialManager.GetSavedEmail() ?? "";
            var savedPassword = _credentialManager.GetSavedPassword() ?? "";

            // Tìm user trong DB và so sánh plain text password
            var user = await _userRepository.GetByEmailAsync(savedEmail);
            if (user != null && user.Password == savedPassword)
            {
                Email = savedEmail;
                LoginPageEvents.RaiseLoginSuccess();
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoLogin] That bai: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }

        return false;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Vui lòng nhập email và mật khẩu.";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // 1. Tìm user trong bảng users
            var user = await _userRepository.GetByEmailAsync(Email.Trim().ToLowerInvariant());
            if (user == null)
            {
                ErrorMessage = "Email hoặc mật khẩu không đúng.";
                return;
            }

            // 2. So sánh plain text password
            if (user.Password != Password)
            {
                ErrorMessage = "Email hoặc mật khẩu không đúng.";
                return;
            }

            // 3. Lưu credentials nếu thành công
            _credentialManager.SaveCredentials(user.Email, user.Password);

            // 4. Chuyển sang màn hình chính
            LoginPageEvents.RaiseLoginSuccess();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Đăng nhập thất bại: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void OpenConfig()
        => LoginPageEvents.RaiseNavigateToConfig();

    [RelayCommand]
    public void BackToLogin()
        => LoginPageEvents.RaiseNavigateToLogin();
}
