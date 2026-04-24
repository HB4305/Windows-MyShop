using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MyShop.Services;

namespace MyShop.Services;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;

    public GeminiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> GenerateDescriptionAsync(string prompt, byte[]? imageBytes = null, string mimeType = "image/jpeg")
    {
        var apiKey = AiConfig.GeminiApiKey;
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            throw new InvalidOperationException("Gemini API Key is not configured by the developer in AiConfig.cs.");
        }

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{AiConfig.GeminiModel}:generateContent?key={apiKey}";

        var requestBody = new GeminiRequest
        {
            SystemInstruction = new GeminiContent
            {
                Parts = new List<GeminiPart> 
                { 
                    new GeminiPart { Text = "You are a professional product description generator. Your goal is to write high-quality sales copy. NEVER ask the user for more information. If an image is provided, analyze it. If not, use the text details. Always provide a full description directly." } 
                }
            },
            Contents = new[]
            {
                new GeminiContent
                {
                    Parts = new List<GeminiPart>()
                }
            }
        };

        requestBody.Contents[0].Parts.Add(new GeminiPart { Text = prompt });

        if (imageBytes != null && imageBytes.Length > 0)
        {
            requestBody.Contents[0].Parts.Add(new GeminiPart
            {
                InlineData = new GeminiInlineData
                {
                    MimeType = mimeType,
                    Data = Convert.ToBase64String(imageBytes)
                }
            });
        }

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API error: {response.StatusCode} - {responseContent}");
        }

        var result = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "No description generated.";
    }

    public async Task<string> AnalyzeItemAsync(byte[] imageBytes, string[] availableCategories, string mimeType = "image/jpeg")
    {
        var categoryList = string.Join(", ", availableCategories);
        var prompt = $@"Analyze this image and return a JSON object for a sport store inventory.
AVAILABLE CATEGORIES: [{categoryList}]

FIELDS TO FILL:
- name: (Professional product name)
- price: (Suggested retail price in USD, number only)
- cost_price: (Suggested cost price, usually 60-70% of price)
- category: (MUST be exactly one from the AVAILABLE CATEGORIES list above. If unsure, pick the most relevant one)
- low_stock_threshold: (Suggested alert level, e.g., 5 or 10)
- color: (Main color)
- description: (Engaging 2-3 sentence sales copy)
- suggested_variants: (List of 3 size/color objects. Use industry-standard sizing based on the detected category:
    * FOOTWEAR/SHOES: Use US numeric sizes (e.g., ""8"", ""9"", ""10.5"") or EU sizes (e.g., ""40"", ""42"").
    * APPAREL/CLOTHING: Use standard letter sizes (e.g., ""S"", ""M"", ""L"", ""XL"", ""XXL"").
    * TENNIS/BADMINTON RACKETS: Use grip sizes (e.g., ""G2"", ""G3"", ""4 1/4"").
    * BALLS (Football/Basketball): Use official sizes (e.g., ""Size 5"", ""Size 7"").
    * OTHER EQUIPMENT: Use ""One Size"" or relevant measurements like ""Standard"", ""Junior"".
    Example: [{{""size"": ""9.5"", ""color"": ""Black"", ""sku"": ""NIKE-BLK-95""}}, {{""size"": ""10"", ""color"": ""Black"", ""sku"": ""NIKE-BLK-10""}}])

IMPORTANT: Return ONLY a raw JSON object. Do NOT include markdown code blocks (```json), conversational text, or any other formatting. Ensure all numbers are valid JSON numbers.";

        return await GenerateDescriptionAsync(prompt, imageBytes, mimeType);
    }

    private class GeminiRequest
    {
        public GeminiContent? SystemInstruction { get; set; }
        public GeminiContent[] Contents { get; set; } = Array.Empty<GeminiContent>();
    }

    private class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; } = new();
    }

    private class GeminiPart
    {
        public string? Text { get; set; }
        public GeminiInlineData? InlineData { get; set; }
    }

    private class GeminiInlineData
    {
        public string MimeType { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }

    private class GeminiResponse
    {
        public GeminiCandidate[]? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }
}
