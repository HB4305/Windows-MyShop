using System.Diagnostics;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Repositories;
using MyShop.Services;
using MyShop.ViewModels;
using Supabase;

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
    /// Gọi khi app khởi động — đọc .env và config.json, đăng ký DI.
    /// </summary>
    public static IServiceProvider Build()
    {
        Services.Clear();

        // ── 1. Đọc .env (giá trị mặc định) ───────────────────────
        var baseDir = AppContext.BaseDirectory;
        var envPath = Path.Combine(baseDir, "MyShop", ".env");
        if (File.Exists(envPath))
            Env.Load(envPath);
        else
            Env.Load(baseDir);

        // ── 2. CredentialManager (Singleton) ──────────────────────
        _credentialManager = new CredentialManager();
        Services.AddSingleton(_credentialManager);

        // ── 3. Supabase URL & Anon Key
        //    Ưu tiên: config.json > .env > exception
        var url = _credentialManager.GetSupabaseUrl()
                  ?? Environment.GetEnvironmentVariable("SUPABASE_URL")
                  ?? throw new InvalidOperationException(
                      "SUPABASE_URL chưa được cấu hình. Vào mục 'Cấu hình server' để nhập.");
        var anonKey = _credentialManager.GetSupabaseAnonKey()
                      ?? Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")
                      ?? throw new InvalidOperationException(
                      "SUPABASE_ANON_KEY chưa được cấu hình. Vào mục 'Cấu hình server' để nhập.");

        // ── 4. Supabase Client (Singleton) ───────────────────────
        Services.AddSingleton(_ => CreateSupabaseClient(url, anonKey));

        // ── 4b. Factory: resolve Client từ provider đã build ───
        Services.AddSingleton<Func<Client>>(_ => () => _provider!.GetRequiredService<Client>());

        // ── 5. Repositories ────────────────────────────────────
        Services.AddScoped<CategoryRepository>();
        Services.AddScoped<SportItemRepository>();
        Services.AddScoped<OrderRepository>();
        Services.AddScoped<SupplyRepository>();

        // ── 6. Services ─────────────────────────────────────────
        Services.AddScoped<CategoryService>();
        Services.AddScoped<SportItemService>();
        Services.AddScoped<OrderService>();
        Services.AddScoped<SupplyService>();

        // ── 7. ViewModels ───────────────────────────────────────
        Services.AddTransient<LoginViewModel>();
        Services.AddTransient<ConfigViewModel>();
        Services.AddTransient<CategoryViewModel>();
        Services.AddTransient<DashboardViewModel>();

        // ── 8. Build và lưu provider ────────────────────────────
        _provider = Services.BuildServiceProvider();
        return _provider;
    }

    /// <summary>
    /// Kiểm tra có JWT Token đã lưu và còn hạn không (cho auto-login).
    /// </summary>
    public static bool HasValidToken()
        => _credentialManager?.HasValidToken() ?? false;

    /// <summary>
    /// Lấy Supabase client hiện tại.
    /// </summary>
    public static Client GetSupabaseClient()
        => _provider!.GetRequiredService<Client>();

    /// <summary>
    /// Lấy CredentialManager hiện tại.
    /// </summary>
    public static CredentialManager GetCredentialManager()
        => _credentialManager!;

    /// <summary>
    /// Build lại Supabase client với config mới (sau khi user đổi server).
    /// </summary>
    public static void RebuildSupabaseClient(string url, string anonKey)
    {
        _credentialManager?.SaveServerConfig(url, anonKey);

        // Gỡ client cũ, tạo client mới
        var descriptor = Services.SingleOrDefault(
            d => d.ServiceType == typeof(Supabase.Client));
        if (descriptor != null)
            Services.Remove(descriptor);

        Services.AddSingleton(_ => CreateSupabaseClient(url, anonKey));
        _provider = Services.BuildServiceProvider();
        Debug.WriteLine("[MauiProgram] Supabase client rebuilt with new config.");
    }

    private static Supabase.Client CreateSupabaseClient(string url, string anonKey)
    {
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false,
        };
        return new Supabase.Client(url, anonKey, options);
    }
}
