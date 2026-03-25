using System;
using System.Diagnostics;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Supabase;
using Uno.Resizetizer;
using MyShop.Views;

namespace MyShop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>Main application window.</summary>
    protected Window? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // ── 1. Khởi tạo Dependency Injection ────────────────────
        Services = MauiProgram.Build();

        // ── 2. Khởi tạo Supabase Client (không blocking UI) ──────
        _ = InitializeSupabaseAsync();

        // ── 3. Cửa sổ chính ──────────────────────────────────────
        MainWindow = new Window();
#if DEBUG
        MainWindow.UseStudio();
#endif

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(CategoryPage), args.Arguments);
        }

        MainWindow.SetWindowIcon();
        MainWindow.Activate();
    }

    private async Task InitializeSupabaseAsync()
    {
        try
        {
            var client = Services.GetRequiredService<Client>();
            await client.InitializeAsync();
            Debug.WriteLine("[App] Supabase client initialized successfully.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] Supabase initialization failed: {ex.Message}");
        }
    }

    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        => throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");

    public static void InitializeLogging()
    {
#if DEBUG
        var factory = LoggerFactory.Create(builder =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#endif
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
