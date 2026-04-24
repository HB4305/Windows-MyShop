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
    Task<string?> PickSavePathAsync(string defaultFileName, string fileTypeLabel, string fileExtension);

    /// <summary>
    /// Shows a native Open dialog for images and returns the selected file path.
    /// </summary>
    Task<string?> PickOpenFileAsync(string fileTypeLabel, string[] fileExtensions);
}
