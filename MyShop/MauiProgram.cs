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

    public static IServiceProvider Build()
    {
        // ── Đọc .env file (nằm trong thư mục MyShop/) ──────────
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), "MyShop", ".env");
        Env.Load(envPath);

        // ── Supabase Client (Singleton) ──────────────────────────
        var url = Environment.GetEnvironmentVariable("SUPABASE_URL")
            ?? throw new InvalidOperationException("SUPABASE_URL environment variable is not set.");
        var anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")
            ?? throw new InvalidOperationException("SUPABASE_ANON_KEY environment variable is not set.");

        Services.AddSingleton(_ =>
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false, // Desktop không cần realtime
            };
            return new Client(url, anonKey, options);
        });

        // ── Repositories ────────────────────────────────────────
        Services.AddScoped<ICategoryRepository, CategoryRepository>();

        // ── Services ────────────────────────────────────────────
        Services.AddScoped<ICategoryService, CategoryService>();

        // ── ViewModels ─────────────────────────────────────────
        Services.AddTransient<CategoryViewModel>();

        return Services.BuildServiceProvider();
    }
}
