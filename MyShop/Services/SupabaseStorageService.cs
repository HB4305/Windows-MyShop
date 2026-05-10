using System.Net.Http.Headers;
using System.Text.Json;

namespace MyShop.Services;

/// <summary>
/// Service to handle file uploads to Supabase Storage.
/// </summary>
public class SupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly string _bucket;

    public SupabaseStorageService()
    {
        _httpClient = new HttpClient();
        _supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "";
        _supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? "";
        _bucket = Environment.GetEnvironmentVariable("SUPABASE_BUCKET") ?? "sport-images";

        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseKey))
        {
            // Fallback or warning - in a real app, you'd handle this better
            System.Diagnostics.Debug.WriteLine("Supabase credentials missing in environment variables.");
        }
    }

    /// <summary>
    /// Uploads a file to Supabase Storage and returns the public URL.
    /// </summary>
    public async Task<string> UploadFileAsync(byte[] bytes, string fileName)
    {
        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseKey))
        {
            throw new InvalidOperationException("Supabase storage is not configured.");
        }

        // Clean filename to be URL safe
        var safeFileName = Guid.NewGuid().ToString() + "_" + fileName;
        var uploadUrl = $"{_supabaseUrl}/storage/v1/object/{_bucket}/{safeFileName}";

        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Add("apikey", _supabaseKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _supabaseKey);
        
        request.Content = new ByteArrayContent(bytes);
        // Set Content-Type based on extension if possible, default to octet-stream
        var extension = Path.GetExtension(fileName).ToLower();
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        });

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Supabase upload failed: {response.StatusCode} - {responseContent}");
        }

        // Return the public URL
        // Supabase public URL format: {url}/storage/v1/object/public/{bucket}/{path}
        return $"{_supabaseUrl}/storage/v1/object/public/{_bucket}/{safeFileName}";
    }
}
