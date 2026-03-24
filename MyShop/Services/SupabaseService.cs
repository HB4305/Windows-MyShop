using Supabase;

namespace MyShop.Services;

public class SupabaseService
{
    private static SupabaseService? _instance;
    public static SupabaseService Instance => _instance ??= new SupabaseService();

    private readonly Client _client;

    public Client Client => _client;

    private SupabaseService()
    {
        var url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? throw new InvalidOperationException("SUPABASE_URL not found");
        var anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? throw new InvalidOperationException("SUPABASE_ANON_KEY not found");

        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
        };

        _client = new Client(url, anonKey, options);
    }

    /// <summary>
    /// Khởi tạo kết nối Supabase. Gọi 1 lần khi app start.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
        System.Diagnostics.Debug.WriteLine("[SupabaseService] Connected successfully!");
    }
}
