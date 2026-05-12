namespace MyShop.Services;

public interface IAiService
{
    /// <summary>
    /// Generates a product description based on a prompt and optional image.
    /// </summary>
    /// <param name="prompt">The textual prompt (specifications).</param>
    /// <param name="imageBytes">Optional image data.</param>
    /// <param name="mimeType">The MIME type of the image (e.g., image/jpeg).</param>
    /// <returns>The generated description.</returns>
    Task<string> GenerateDescriptionAsync(string prompt, byte[]? imageBytes = null, string mimeType = "image/jpeg");

    /// <summary>
    /// Analyzes an image and returns structured product data in JSON format.
    /// </summary>
    Task<string> AnalyzeItemAsync(byte[] imageBytes, string[] availableCategories, string? categoryHint = null, string mimeType = "image/jpeg");

    /// <summary>
    /// Repairs a JSON object to match the expected schema and constraints.
    /// </summary>
    Task<string> RepairItemJsonAsync(string rawJson, string[] validationErrors, string[] availableCategories);

    /// <summary>
    /// Runs a generic prompt without images.
    /// </summary>
    Task<string> RunPromptAsync(string systemInstruction, string prompt, System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Parses a natural language search query into structured filter data.
    /// </summary>
    Task<Models.Ai.AiSearchFilter?> ParseSearchQueryAsync(string query);
}
