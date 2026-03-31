using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Services;

namespace MyShop.ViewModels;

/// <summary>
/// Sự kiện khi lưu cấu hình thành công → quay lại login.
/// </summary>
public static class ConfigPageEvents
{
    public static event Action? OnConfigSaved;
    public static event Action? OnBack;
    public static void RaiseConfigSaved() => OnConfigSaved?.Invoke();
    public static void RaiseBack() => OnBack?.Invoke();
}

public partial class ConfigViewModel : ObservableObject
{
    private readonly CredentialManager _credentialManager;

    public ConfigViewModel(CredentialManager credentialManager)
    {
        _credentialManager = credentialManager;

        // Load cấu hình hiện tại
        SupabaseUrl = _credentialManager.GetSupabaseUrl() ?? string.Empty;
        SupabaseAnonKey = _credentialManager.GetSupabaseAnonKey() ?? string.Empty;
    }

    [ObservableProperty]
    private string _supabaseUrl = string.Empty;

    [ObservableProperty]
    private string _supabaseAnonKey = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    [RelayCommand]
    public void Save()
    {
        if (string.IsNullOrWhiteSpace(SupabaseUrl))
        {
            Message = "Vui lòng nhập Supabase URL.";
            IsSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(SupabaseAnonKey))
        {
            Message = "Vui lòng nhập Supabase Anon Key.";
            IsSuccess = false;
            return;
        }

        _credentialManager.SaveServerConfig(SupabaseUrl.Trim(), SupabaseAnonKey.Trim());
        Message = "Lưu cấu hình thành công!";
        IsSuccess = true;

        // Báo App reload Supabase client với config mới
        ConfigPageEvents.RaiseConfigSaved();
    }

    [RelayCommand]
    public void Back()
        => ConfigPageEvents.RaiseBack();
}
