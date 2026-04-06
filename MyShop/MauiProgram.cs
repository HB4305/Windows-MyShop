using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Repositories;
using MyShop.Services;
using MyShop.ViewModels;
using Npgsql;

namespace MyShop;

/// <summary>
/// Cấu hình Dependency Injection cho toàn bộ ứng dụng.
/// </summary>
public static class MauiProgram
{
    public static IServiceCollection Services { get; private set; } = new ServiceCollection();

    private static CredentialManager? _credentialManager;
    private static IServiceProvider? _provider;

    /// <summary>
    /// Gọi khi app khởi động — đăng ký DI.
    /// </summary>
    public static IServiceProvider Build()
    {
        Services.Clear();

        // ── 1. CredentialManager (Singleton) ──────────────────────
        _credentialManager = new CredentialManager();
        Services.AddSingleton(_credentialManager);

        // ── 2. DbConnectionFactory (Singleton) ─────────────────────
        Services.AddSingleton<DbConnectionFactory>();

        // ── 3. Repositories ────────────────────────────────────────
        Services.AddScoped<CategoryRepository>();
        Services.AddScoped<SportItemRepository>();
        Services.AddScoped<OrderRepository>();
        Services.AddScoped<SupplyRepository>();
        Services.AddScoped<ReportRepository>();
        Services.AddScoped<CustomerOrderRepository>();
        Services.AddScoped<OrderDetailRepository>();
        Services.AddScoped<UserRepository>();
        Services.AddSingleton<SettingsManager>();

        // ── 4. Services ────────────────────────────────────────────
        Services.AddScoped<CategoryService>();
        Services.AddScoped<SportItemService>();
        Services.AddScoped<OrderService>();
        Services.AddScoped<SupplyService>();
        Services.AddScoped<ReportService>();
        Services.AddScoped<CustomerOrderService>();
        Services.AddScoped<OrderDetailService>();

        // ── 5. ViewModels ─────────────────────────────────────────
        Services.AddTransient<LoginViewModel>();
        Services.AddTransient<ConfigViewModel>();
        Services.AddTransient<CategoryViewModel>();
        Services.AddTransient<DashboardViewModel>();
        Services.AddTransient<ReportViewModel>();
        Services.AddTransient<SportItemViewModel>();
        Services.AddTransient<SportItemDetailViewModel>();
        Services.AddTransient<CustomerOrderViewModel>();
        Services.AddTransient<OrderDetailViewModel>();
        Services.AddTransient<SettingsViewModel>();

        // ── 6. Build ───────────────────────────────────────────────
        _provider = Services.BuildServiceProvider();
        return _provider;
    }

    /// <summary>
    /// Lấy CredentialManager hiện tại.
    /// </summary>
    public static CredentialManager GetCredentialManager()
        => _credentialManager!;
}
