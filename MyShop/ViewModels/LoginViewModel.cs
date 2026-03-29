using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services;
using Supabase;

namespace MyShop.ViewModels;

/// <summary>
/// Sự kiện điều hướng từ LoginPage.
/// </summary>
public static class LoginPageEvents
{
    public static event Action? OnLoginSuccess;
    public static event Action? OnNavigateToConfig;
    public static event Action? OnNavigateToSignUp;
    public static event Action? OnNavigateToLogin;
    public static void RaiseLoginSuccess() => OnLoginSuccess?.Invoke();
    public static void RaiseNavigateToConfig() => OnNavigateToConfig?.Invoke();
    public static void RaiseNavigateToSignUp() => OnNavigateToSignUp?.Invoke();
    public static void RaiseNavigateToLogin() => OnNavigateToLogin?.Invoke();
}

public partial class LoginViewModel : ObservableObject
{
    private readonly CredentialManager _credentialManager;
    private readonly Func<Client> _clientFactory;

    public LoginViewModel(CredentialManager credentialManager, Func<Client> clientFactory)
    {
        _credentialManager = credentialManager;
        _clientFactory = clientFactory;
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
    private bool _rememberMe = true;

    [ObservableProperty]
    private bool _keepSessionActive = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Phiên bản ứng dụng hiển thị ở màn hình login.
    /// Format: "Version 1.0.0 · © 2026 ProSport"
    /// </summary>
    public string AppVersion
    {
        get
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            var verStr = ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
            return $"Version {verStr} · © {DateTime.Now.Year} ProSport";
        }
    }

    /// <summary>
    /// Đăng nhập bằng JWT Token đã lưu (auto-login).
    /// Thử khôi phục session từ token, không cần nhập password.
    /// </summary>
    public async Task<bool> TryAutoLoginAsync()
    {
        var tokens = _credentialManager.LoadTokens();
        if (!tokens.HasValue)
            return false;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var client = _clientFactory();
            await client.InitializeAsync();

            // Khôi phục session từ JWT tokens đã lưu
            var (accessToken, refreshToken) = tokens.Value;
            await client.Auth.SetSession(accessToken, refreshToken);

            // Kiểm tra session còn hợp lệ không
            if (client.Auth.CurrentSession?.User is not null)
            {
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

            // 1. Khởi tạo Supabase client
            var client = _clientFactory();
            await client.InitializeAsync();

            // 2. Gọi Supabase Auth đăng nhập
            //    Supabase server verify password bằng PBKDF2 internally
            //    Trả về JWT session token
            var session = await client.Auth.SignInWithPassword(Email, Password);

            if (session?.User is not null)
            {
                // 3. Lấy JWT Tokens từ session
                var accessToken = session.AccessToken;
                var refreshToken = session.RefreshToken ?? "";

                // 4. Tạo BCrypt hash của password
                //    Dùng để verify local (không cần gọi server lần sau)
                var bcryptHash = CredentialManager.HashPassword(Password);

                // 5. Lưu credentials nếu Remember Me = true
                if (RememberMe)
                    _credentialManager.SaveCredentials(Email, bcryptHash, accessToken, refreshToken);
                else
                    _credentialManager.ClearCredentials();

                // 6. Chuyển sang màn hình chính
                LoginPageEvents.RaiseLoginSuccess();
            }
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
    public void OpenSignUp()
        => LoginPageEvents.RaiseNavigateToSignUp();

    [RelayCommand]
    public void BackToLogin()
        => LoginPageEvents.RaiseNavigateToLogin();

    /// <summary>
    /// Đăng ký tài khoản owner mới (lần đầu).
    /// </summary>
    [RelayCommand]
    public async Task SignUpAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Vui lòng nhập email và mật khẩu.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Mật khẩu xác nhận không khớp.";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var client = _clientFactory();
            await client.InitializeAsync();

            // 1. Đăng ký tài khoản với Supabase Auth
            var session = await client.Auth.SignUp(Email, Password);

            if (session?.User is not null)
            {
                // 2. Tạo BCrypt hash
                var bcryptHash = CredentialManager.HashPassword(Password);

                // 3. Lưu credentials
                var accessToken = session.AccessToken;
                var refreshToken = session.RefreshToken ?? "";
                _credentialManager.SaveCredentials(Email, bcryptHash, accessToken, refreshToken);

                // 4. Chuyển sang màn hình chính
                LoginPageEvents.RaiseLoginSuccess();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Đăng ký thất bại: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
