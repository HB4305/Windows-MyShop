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
    /// Đăng nhập tự động bằng credentials đã lưu (EncryptedPassword + PasswordHash).
    /// Không cần DB trong bước xác thực cuối — chỉ dùng hash đã lưu.
    /// Nếu thành công → RaiseLoginSuccess().
    /// Nếu thất bại → form vẫn hiển thị với email + password đã điền sẵn.
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
            var savedPassword = _credentialManager.GetSavedPassword();
            if (string.IsNullOrEmpty(savedPassword))
                return false;

            // 1. Xác thực bằng hash đã lưu — không cần DB
            if (!_credentialManager.ValidatePassword(savedPassword))
                return false;

            // 2. Gọi DB để đảm bảo user vẫn tồn tại và không bị disable
            var user = await _userRepository.GetByEmailAsync(savedEmail);
            if (user == null)
                return false;

            // 3. Điền sẵn email + password vào form (nếu cần nhìn thấy)
            Email = savedEmail;
            Password = savedPassword;
            LoginPageEvents.RaiseLoginSuccess();
            return true;
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

            // 1. Hash password nhập vào để so sánh với DB (DB lưu hash SHA256 base64)
            var hashedInput = CredentialManager.ComputeHash(Password);

            // 2. Tìm user trong bảng users — DB trả về SHA256 hash
            var user = await _userRepository.GetByEmailAsync(Email.Trim().ToLowerInvariant());
            if (user == null)
            {
                ErrorMessage = "Email hoặc mật khẩu không đúng.";
                return;
            }

            // 3. So sánh hash của input với hash trong DB
            if (user.Password != hashedInput)
            {
                ErrorMessage = "Email hoặc mật khẩu không đúng.";
                return;
            }

            // 4. Lưu credentials: plain text (DPAPI) + hash (để auto-login)
            _credentialManager.SaveCredentials(user.Email, Password);

            // 5. Chuyển sang màn hình chính
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
