using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views;

namespace MyShop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    protected Window? MainWindow { get; private set; }
    private Frame? _rootFrame;

    public App() { this.InitializeComponent(); }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Services = MauiProgram.Build();

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
        _rootFrame = rootFrame;

        // Đăng ký event handlers cho authentication flow
        LoginPageEvents.OnLoginSuccess     += OnLoginSuccess;
        LoginPageEvents.OnNavigateToConfig += OnNavigateToConfig;
        ConfigPageEvents.OnConfigSaved     += OnConfigSaved;
        ConfigPageEvents.OnBack            += OnConfigBack;
        LoginPageEvents.OnNavigateToSignUp += OnNavigateToSignUp;
        LoginPageEvents.OnNavigateToLogin  += OnNavigateToLogin;

        // Luôn khởi đầu tại LoginPage — auth là bắt buộc
        rootFrame.Navigate(typeof(LoginPage), args.Arguments);
        MainWindow.SetWindowIcon();
        MainWindow.Activate();
    }

    private void OnLoginSuccess() => NavigateToMain();

    private void OnNavigateToConfig()
        => _rootFrame?.Navigate(typeof(ConfigPage));


    private void OnNavigateToSignUp()
        => _rootFrame?.Navigate(typeof(SignUpPage));

    private void OnNavigateToLogin()
        => _rootFrame?.Navigate(typeof(LoginPage));

    private void OnConfigSaved()
    {
        var credMgr = Services.GetRequiredService<CredentialManager>();
        var url = credMgr.GetSupabaseUrl();
        var key = credMgr.GetSupabaseAnonKey();
        if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(key))
            MauiProgram.RebuildSupabaseClient(url, key);
        _rootFrame?.Navigate(typeof(LoginPage));
    }

    private void OnConfigBack()
        => _rootFrame?.GoBack();

    private void NavigateToMain()
    {
        if (_rootFrame == null) return;
        _rootFrame.BackStack.Clear();
        _rootFrame.Navigate(typeof(CategoryPage));
    }

    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        => throw new InvalidOperationException("Failed to load " + e.SourcePageType.FullName + ": " + e.Exception);

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
