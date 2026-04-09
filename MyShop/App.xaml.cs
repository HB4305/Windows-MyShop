using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Services;
using MyShop.ViewModels;
using MyShop.Views;

namespace MyShop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public Window? MainWindow { get; private set; }
    private Frame? _rootFrame;

    public App()
    {
        // Set global culture to Vietnamese for correct date and number formatting
        var culture = new System.Globalization.CultureInfo("en-US");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

        // Uno-specific primary language override
        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "vi-VN";

        this.InitializeComponent();
    }

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

        // Register event handlers for authentication flow
        LoginPageEvents.OnLoginSuccess += OnLoginSuccess;
        LoginPageEvents.OnNavigateToConfig += OnNavigateToConfig;
        ConfigPageEvents.OnConfigSaved += OnConfigSaved;
        ConfigPageEvents.OnBack += OnConfigBack;
        LoginPageEvents.OnNavigateToLogin += OnNavigateToLogin;
        ShellPageEvents.OnLogout += OnLogout;

        // Flow: ConfigScreen → Login → Dashboard
        // No config yet → ConfigPage; has config → LoginPage
        var credMgr = Services.GetRequiredService<CredentialManager>();
        if (credMgr.HasDatabaseConfig())
            rootFrame.Navigate(typeof(LoginPage), args.Arguments);
        else
            rootFrame.Navigate(typeof(ConfigPage), args.Arguments);

        MainWindow.SetWindowIcon();
        MainWindow.Activate();
    }

    private void OnLoginSuccess() => NavigateToMain();

    private void OnNavigateToConfig()
        => _rootFrame?.Navigate(typeof(ConfigPage));

    private void OnNavigateToLogin()
        => _rootFrame?.Navigate(typeof(LoginPage));

    private void OnConfigSaved()
    {
        // Invalidate connection cache to use new config
        var connFactory = Services.GetRequiredService<DbConnectionFactory>();
        connFactory.InvalidateCache();
        _rootFrame?.Navigate(typeof(LoginPage));
    }

    private void OnConfigBack()
        => _rootFrame?.GoBack();

    private void NavigateToMain()
    {
        if (_rootFrame == null) return;
        _rootFrame.BackStack.Clear();
        _rootFrame.Navigate(typeof(ShellPage));
    }

    private void OnLogout()
    {
        if (_rootFrame == null) return;
        // Clear saved credentials, KEEP email for convenience
        var credMgr = Services.GetRequiredService<CredentialManager>();
        credMgr.ClearCredentials();
        // Clear current user session
        var currentUser = Services.GetRequiredService<CurrentUserService>();
        currentUser.Clear();
        // Clear backstack and go back to Login
        _rootFrame.BackStack.Clear();
        _rootFrame.Navigate(typeof(LoginPage));
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
