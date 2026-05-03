using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Repositories;
using MyShop.Services;
using MyShop.ViewModels;
using Npgsql;

namespace MyShop;

/// <summary>
/// Dependency Injection configuration for the entire application.
/// </summary>
public static class MauiProgram
{
    public static IServiceCollection Services { get; private set; } = new ServiceCollection();

    private static CredentialManager? _credentialManager;
    private static IServiceProvider? _provider;

    /// <summary>
    /// Called on app startup to register DI services.
    /// </summary>
    public static IServiceProvider Build()
    {
        // Load environment variables from .env file
        Utils.EnvLoader.Load();

        Services.Clear();

        // ── 1. CredentialManager (Singleton) ──────────────────────
        _credentialManager = new CredentialManager();
        Services.AddSingleton(_credentialManager);

        // ── 1b. CurrentUserService (Singleton) ───────────────────
        // Stores the currently logged-in user information for RBAC permissions.
        Services.AddSingleton<CurrentUserService>();

        // ── 2. DbConnectionFactory (Singleton) ─────────────────────
        Services.AddSingleton<DbConnectionFactory>();

        // ── 3. Repositories ────────────────────────────────────────
        Services.AddScoped<CategoryRepository>();
        Services.AddScoped<SportItemRepository>();
        Services.AddScoped<OrderRepository>();
        Services.AddScoped<SupplyRepository>();
        Services.AddScoped<SupplierRepository>();
        Services.AddScoped<ReportRepository>();
        Services.AddScoped<CustomerOrderRepository>();
        Services.AddScoped<OrderDetailRepository>();
        Services.AddScoped<UserRepository>();
        Services.AddScoped<CustomerRepository>();
        Services.AddSingleton<SettingsManager>();

        // ── 4. Services ────────────────────────────────────────────
        Services.AddScoped<CategoryService>();
        Services.AddScoped<SportItemService>();
        Services.AddScoped<OrderService>();
        Services.AddScoped<SupplyService>();
        Services.AddScoped<ReportService>();
        Services.AddScoped<CustomerOrderService>();
        Services.AddScoped<OrderDetailService>();
        Services.AddScoped<CustomerService>();
        Services.AddSingleton<IInvoiceService, InvoiceService>();
        Services.AddSingleton<IFilePickerService>(new FilePickerServiceFactory().Create());
        Services.AddSingleton<IAiService, GeminiService>();

        // ── 5. ViewModels ─────────────────────────────────────────
        Services.AddTransient<LoginViewModel>();
        Services.AddTransient<ConfigViewModel>();
        Services.AddTransient<CategoryViewModel>();
        Services.AddTransient<DashboardViewModel>();
        Services.AddTransient<ReportViewModel>();
        Services.AddTransient<PosViewModel>();
        Services.AddTransient<SportItemViewModel>();
        Services.AddTransient<SportItemDetailViewModel>();
        Services.AddTransient<CustomerOrderViewModel>();
        Services.AddTransient<OrderDetailViewModel>();
        Services.AddTransient<CustomerViewModel>();
        Services.AddTransient<SuppliersViewModel>();
        Services.AddTransient<SettingsViewModel>();

        // ── 6. Build ───────────────────────────────────────────────
        _provider = Services.BuildServiceProvider();
        return _provider;
    }

    /// <summary>
    /// Gets the current CredentialManager.
    /// </summary>
    public static CredentialManager GetCredentialManager()
        => _credentialManager!;
}
