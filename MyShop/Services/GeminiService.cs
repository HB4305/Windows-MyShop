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
        const string systemInstruction = "You are a professional product description generator. Your goal is to write high-quality sales copy. NEVER ask the user for more information. If an image is provided, analyze it. If not, use the text details. Always provide a full description directly.";
        return await SendPromptAsync(systemInstruction, prompt, imageBytes, mimeType);
    }

    public async Task<string> AnalyzeItemAsync(byte[] imageBytes, string[] availableCategories, string? categoryHint = null, string mimeType = "image/jpeg")
    {
        var categoryList = string.Join(", ", availableCategories);
        var hintBlock = string.IsNullOrWhiteSpace(categoryHint)
            ? ""
            : $"\nCATEGORY HINT: {categoryHint}\n(Use it only if it matches the image.)\n";
        var prompt = $@"Analyze this image and return a JSON object for a sport store inventory.
AVAILABLE CATEGORIES: [{categoryList}]
{hintBlock}

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

ADDITIONAL FIELDS:
- confidence: (number 0-1, overall confidence)
- reasons: (array of short strings, why you made the choices)
- field_confidence: (object with per-field confidence 0-1, keys: name, price, cost_price, category, low_stock_threshold, color, description, suggested_variants)
- field_reasons: (object with short reasons per field, same keys as field_confidence)

IMPORTANT: Return ONLY a raw JSON object. Do NOT include markdown code blocks (```json), conversational text, or any other formatting. Ensure all numbers are valid JSON numbers.";

        const string systemInstruction = "You are a retail inventory analyst. You must return valid JSON that matches the requested schema and categories. Do not add extra keys.";
        return await SendPromptAsync(systemInstruction, prompt, imageBytes, mimeType);
    }

    public async Task<string> RepairItemJsonAsync(string rawJson, string[] validationErrors, string[] availableCategories)
    {
        var categoryList = string.Join(", ", availableCategories);
        var errors = string.Join("\n", validationErrors.Select(e => $"- {e}"));
        var prompt = $@"You are given a JSON object that is invalid or inconsistent with rules. Fix it.

AVAILABLE CATEGORIES: [{categoryList}]

VALIDATION ERRORS:
{errors}

JSON TO FIX:
{rawJson}

Return ONLY the corrected JSON object. Do not add markdown or extra text.";

        const string systemInstruction = "You are a strict JSON repair tool. Output must be valid JSON matching the required schema and constraints.";
        return await SendPromptAsync(systemInstruction, prompt, null, "image/jpeg");
    }

    public async Task<string> RunPromptAsync(string systemInstruction, string prompt, System.Threading.CancellationToken ct = default)
    {
        return await SendPromptAsync(systemInstruction, prompt, null, "image/jpeg", ct);
    }

    private async Task<string> SendPromptAsync(string systemInstruction, string prompt, byte[]? imageBytes, string mimeType, System.Threading.CancellationToken ct = default)
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
                    new GeminiPart { Text = systemInstruction }
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
            // Optimize for Gemini: 1024px is plenty for analysis and keeps payload small
            var optimizedBytes = Utils.ImageHelper.CompressAndResize(imageBytes, maxDimension: 1024, quality: 75);

            requestBody.Contents[0].Parts.Add(new GeminiPart
            {
                InlineData = new GeminiInlineData
                {
                    MimeType = mimeType,
                    Data = Convert.ToBase64String(optimizedBytes)
                }
            });
        }

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct);
        var responseContent = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API error: {response.StatusCode}. Details: {responseContent}");
        }

        var result = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        var candidate = result?.Candidates?.FirstOrDefault();
        if (candidate == null)
        {
            return "No response from AI (Candidates list is empty). This might be due to safety filters.";
        }

        if (candidate.FinishReason != null && candidate.FinishReason != "STOP")
        {
            return $"AI stopped generating. Reason: {candidate.FinishReason}. This usually happens when content is blocked or safety filters are triggered.";
        }

        return candidate.Content?.Parts?.FirstOrDefault()?.Text ?? "No content generated.";
    }

    public async Task<Models.Ai.AiSearchFilter?> ParseSearchQueryAsync(string query)
    {
        string systemInstruction = @"You are a search query parser for a sports shop in Vietnam.
Extract the keyword, min_price, and max_price from the user's natural language search query.
Return the result STRICTLY as a raw JSON object, without any markdown code blocks.
If the user mentions terms like 'k', 'lít', 'củ', 'triệu', convert them to standard VND numbers.
Rules:
- 'k' or 'nghìn' = 1,000
- 'lít' = 100,000
- 'củ' or 'tr' or 'triệu' = 1,000,000
- If no price is mentioned, keep min_price and max_price as null.
- Remove price text from the keyword, keep only the actual product name/category.
Example 1: 'vợt cầu lông dưới 2 củ rưỡi' -> { ""keyword"": ""vợt cầu lông"", ""min_price"": null, ""max_price"": 2500000, ""reasoning"": ""Giới hạn dưới 2.500.000đ"" }
Example 2: 'áo đá bóng từ 100k đến 300k' -> { ""keyword"": ""áo đá bóng"", ""min_price"": 100000, ""max_price"": 300000, ""reasoning"": ""Khung giá 100k - 300k"" }";

        string rawJson = await RunPromptAsync(systemInstruction, $"Query: \"{query}\"");

        try
        {
            if (rawJson.StartsWith("```"))
            {
                int firstNewline = rawJson.IndexOf('\n');
                int lastBackticks = rawJson.LastIndexOf("```");
                if (firstNewline != -1 && lastBackticks > firstNewline)
                {
                    rawJson = rawJson.Substring(firstNewline + 1, lastBackticks - firstNewline - 1).Trim();
                }
            }

            var filter = JsonSerializer.Deserialize<Models.Ai.AiSearchFilter>(rawJson);
            return filter;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AI Search Filter Parse Error: {ex.Message}");
            return null;
        }
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
        public string? FinishReason { get; set; }
    }
}
