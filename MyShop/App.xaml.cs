using System;
using Microsoft.Extensions.Logging;
using Uno.Resizetizer;
using System.Diagnostics; // Để dùng Debug.WriteLine
using System.Threading.Tasks; // Để dùng Task
using Npgsql; // Thư viện kết nối Postgres
using DotNetEnv; // Thư viện đọc file .env

namespace MyShop;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();
#if DEBUG
        MainWindow.UseStudio();
#endif

        // --- ĐOẠN CODE TEST KẾT NỐI BẮT ĐẦU TẠI ĐÂY ---
        TestSupabaseConnection();
        // --- ĐOẠN CODE TEST KẾT NỐI KẾT THÚC TẠI ĐÂY ---

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(MainPage), args.Arguments);
        }

        MainWindow.SetWindowIcon();
        MainWindow.Activate();
    }

    /// <summary>
    /// Hàm test kết nối nhanh tới Supabase
    /// </summary>
    private void TestSupabaseConnection()
    {
        Task.Run(async () =>
        {
            try
            {
                // 1. Load file .env
                DotNetEnv.Env.Load();
                var connectionString = DotNetEnv.Env.GetString("DATABASE_URL");

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("❌ LỖI: Không tìm thấy DATABASE_URL trong file .env");
                    return;
                }

                // 2. Thử mở kết nối
                using var conn = new NpgsqlConnection(connectionString);
                Console.WriteLine("⏳ Đang thử kết nối tới Supabase...");
                await conn.OpenAsync();

                // 3. Chạy thử 1 câu lệnh SQL đơn giản
                using var cmd = new NpgsqlCommand("SELECT version();", conn);
                var version = await cmd.ExecuteScalarAsync();

                Console.WriteLine("✅ KẾT NỐI THÀNH CÔNG!");
                Console.WriteLine($"ℹ️ Database Version: {version}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ KẾT NỐI THẤT BẠI: {ex.Message}");
            }
        });
    }
    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

    /// <summary>
    /// Configures global Uno Platform logging
    /// </summary>
    public static void InitializeLogging()
    {
#if DEBUG
        // Logging is disabled by default for release builds, as it incurs a significant
        // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
        // is a concern for your application, keep this disabled. If you're running on the web or
        // desktop targets, you can use URL or command line parameters to enable it.
        //
        // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

        var factory = LoggerFactory.Create(builder =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());

            // Log to the Visual Studio Debug console
            builder.AddConsole();
#else
            builder.AddConsole();
#endif

            // Exclude logs below this level
            builder.SetMinimumLevel(LogLevel.Information);

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);

            // Generic Xaml events
            // builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace );

            // Layouter specific messages
            // builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

            // builder.AddFilter("Windows.Storage", LogLevel.Debug );

            // Binding related messages
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );

            // Binder memory references tracking
            // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

            // DevServer and HotReload related
            // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

            // Debug JS interop
            // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
