using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Repositories;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// Navigation events from LoginPage.
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
    private readonly CurrentUserService _currentUserService;

    public LoginViewModel(
        CredentialManager credentialManager,
        UserRepository userRepository,
        CurrentUserService currentUserService)
    {
        _credentialManager = credentialManager;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        Email = _credentialManager.GetSavedEmail() ?? string.Empty;
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// App version displayed on the login screen.
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
    /// Attempts to auto-login using credentials saved in config.json.
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

            // 1. Validate stored hash — no DB needed
            if (!_credentialManager.ValidatePassword(savedPassword))
                return false;

            // 2. Verify user still exists in DB
            var user = await _userRepository.GetByEmailAsync(savedEmail);
            if (user == null)
                return false;

            // 3. Restore session
            var (savedId, savedRole) = _credentialManager.GetSavedUserInfo();
            if (savedId.HasValue && !string.IsNullOrEmpty(savedRole))
            {
                _currentUserService.SetUser(savedId.Value, savedEmail, savedRole);
            }
            else
            {
                _currentUserService.SetUser(user.Id, user.Email, user.Role ?? "sale");
            }

            Email = savedEmail;
            Password = savedPassword;
            LoginPageEvents.RaiseLoginSuccess();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoLogin] Failed: {ex.Message}");
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
            ErrorMessage = "Please enter email and password.";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // 1. Find user in the users table
            var user = await _userRepository.GetByEmailAsync(Email.Trim().ToLowerInvariant());
            if (user == null)
            {
                ErrorMessage = "Email or password is incorrect.";
                return;
            }

            // 2. Compare SHA256 hashes
            var hashedInput = CredentialManager.ComputeHash(Password);
            if (user.Password != hashedInput)
            {
                ErrorMessage = "Email or password is incorrect.";
                return;
            }

            // 3. Save credentials and session
            _credentialManager.SaveCredentials(user.Email, Password, user.Id, user.Role ?? "sale");
            _currentUserService.SetUser(user.Id, user.Email, user.Role ?? "sale");

            // 4. Navigate to main screen
            LoginPageEvents.RaiseLoginSuccess();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
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
