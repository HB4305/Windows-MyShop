namespace MyShop.Services;

/// <summary>
/// Developer-controlled AI configurations.
/// API Key is provided by the application developer, not the end user.
/// </summary>
public static class AiConfig
{
    public static string GeminiApiKey => Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
    
    public static string GeminiModel => Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.0-flash-exp";
}
