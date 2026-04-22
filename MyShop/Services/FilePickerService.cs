namespace MyShop.Services;

/// <summary>
/// Cross-platform file save picker abstraction.
/// Returns the chosen path, or null / empty if the user cancelled.
/// </summary>
public interface IFilePickerService
{
    /// <summary>
    /// Shows a native Save dialog and returns the selected file path.
    /// </summary>
    /// <param name="defaultFileName">Suggested file name (e.g. "Invoice-ATH-00042.pdf")</param>
    /// <param name="fileTypeLabel">Display label for the file type (e.g. "PDF Document")</param>
    /// <param name="fileExtension">Extension including dot (e.g. ".pdf")</param>
    /// <returns>The full path the user chose, or null if cancelled.</returns>
    Task<string?> PickSavePathAsync(string defaultFileName, string fileTypeLabel, string fileExtension);
}
