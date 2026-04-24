namespace MyShop.Services;

/// <summary>
/// Factory that returns the correct file picker for the current platform.
/// Register this as a singleton in DI.
/// </summary>
public sealed class FilePickerServiceFactory
{
#if WINDOWS || NET10_0_DESKTOP || HAS_UNO_SKIA
    private static readonly IFilePickerService _instance = new FilePicker.WindowsFilePickerService();
#elif __MACOS__
    private static readonly IFilePickerService _instance = new FilePicker.MacFilePickerService();
#else
    private static readonly IFilePickerService _instance = new DefaultFilePickerService();
#endif

    public IFilePickerService Create() => _instance;
}

/// <summary>
/// Fallback for unknown platforms — writes to MyDocuments.
/// </summary>
internal sealed class DefaultFilePickerService : IFilePickerService
{
    public Task<string?> PickSavePathAsync(string defaultFileName, string fileTypeLabel, string fileExtension)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(docs, defaultFileName);
        return Task.FromResult<string?>(path);
    }

    public Task<string?> PickOpenFileAsync(string fileTypeLabel, string[] fileExtensions) => Task.FromResult<string?>(null);
}