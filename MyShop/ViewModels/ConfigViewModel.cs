using System.Reflection;
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
    private readonly DbConnectionFactory _connFactory;

    public ConfigViewModel(CredentialManager credentialManager, DbConnectionFactory connFactory)
    {
        _credentialManager = credentialManager;
        _connFactory = connFactory;

        // Load cấu hình hiện tại
        var (host, port, dbName, username, password) = _credentialManager.GetDatabaseConfig();
        DbHost = host ?? string.Empty;
        DbPort = port > 0 ? port.ToString() : "6543";
        DbName = dbName ?? string.Empty;
        DbUsername = username ?? string.Empty;
        DbPassword = password ?? string.Empty;
    }

    [ObservableProperty]
    private string _dbHost = "localhost";

    [ObservableProperty]
    private string _dbPort = "6543";

    [ObservableProperty]
    private string _dbName = string.Empty;

    [ObservableProperty]
    private string _dbUsername = string.Empty;

    [ObservableProperty]
    private string _dbPassword = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    public static string AppVersion
    {
        get
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            var verStr = ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
            return $"Version {verStr} · © {DateTime.Now.Year} ProSport";
        }
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(DbHost))
        {
            Message = "Vui lòng nhập Host.";
            IsSuccess = false;
            return;
        }
        if (string.IsNullOrWhiteSpace(DbName))
        {
            Message = "Vui lòng nhập Database Name.";
            IsSuccess = false;
            return;
        }
        if (string.IsNullOrWhiteSpace(DbUsername))
        {
            Message = "Vui lòng nhập Username.";
            IsSuccess = false;
            return;
        }

        try
        {
            IsLoading = true;
            Message = "Đang kiểm tra kết nối...";
            IsSuccess = false;

            // Test kết nối trước khi lưu (dùng giá trị từ form)
            var port = int.TryParse(DbPort, out var p) ? p : 5432;
            var error = await _connFactory.TestConnectionAsync(
                DbHost.Trim(), port, DbName.Trim(), DbUsername.Trim(), DbPassword);
            if (error != null)
            {
                Message = $"Kết nối thất bại: {error}";
                IsSuccess = false;
                return;
            }

            // Lưu config
            _credentialManager.SaveDatabaseConfig(
                DbHost.Trim(), port, DbName.Trim(), DbUsername.Trim(), DbPassword);

            // Invalidate cache để rebuild connection
            _connFactory.InvalidateCache();

            Message = "Lưu cấu hình thành công!";
            IsSuccess = true;

            // Báo App đã lưu config → quay lại login
            ConfigPageEvents.RaiseConfigSaved();
        }
        catch (Exception ex)
        {
            Message = $"Lỗi: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void Back()
        => ConfigPageEvents.RaiseBack();
}
