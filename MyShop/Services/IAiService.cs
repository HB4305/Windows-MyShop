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
    Task<string> AnalyzeItemAsync(byte[] imageBytes, string[] availableCategories, string mimeType = "image/jpeg");
}
