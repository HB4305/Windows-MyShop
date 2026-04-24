namespace MyShop.Services.FilePicker;

#if __MACOS__

using System.Runtime.InteropServices;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

/// <summary>
/// macOS implementation using native NSSavePanel via ObjC runtime interop.
/// </summary>
internal sealed class MacFilePickerService : IFilePickerService
{
    public Task<string?> PickSavePathAsync(string defaultFileName, string fileTypeLabel, string fileExtension)
    {
        var ext = fileExtension.StartsWith('.') ? fileExtension : $".{fileExtension}";

        // Run on macOS main thread (required for UI)
        return InvokeOnMainThread(() =>
        {
            var panel = new NSSavePanel
            {
                NameFieldStringValue = defaultFileName,
                Message = "Choose where to save the invoice",
                AllowedContentTypes = new[]
                {
                    // UTType based on extension
                    GetUtType(ext)
                },
                CanCreateDirectories = true,
                ShowsTagField = false,
            };

            var result = panel.RunModal();

            if (result == 1)  // NSModalResponse.OK
            {
                var path = panel.Url?.Path;
                return path;
            }
            return null;
        });
    }

    private static CoreServices.UTType GetUtType(string extension)
    {
        // Return the appropriate UTType
        var ext = extension.TrimStart('.').ToLowerInvariant();
        return ext switch
        {
            "pdf" => CoreServices.UTType.Pdf,
            "xps" => CoreServices.UTType.XpsDocument,
            _     => CoreServices.UTType.Data,
        };
    }

    private static Task<T> InvokeOnMainThread<T>(Func<T> action)
    {
        var tcs = new TaskCompletionSource<T>();
        DispatchQueue.MainQueue.DispatchAsync(() =>
        {
            try
            {
                tcs.SetResult(action());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs;
    }

    public Task<string?> PickOpenFileAsync(string fileTypeLabel, string[] fileExtensions)
    {
        return InvokeOnMainThread(() =>
        {
            var panel = new NSOpenPanel
            {
                CanChooseFiles = true,
                CanChooseDirectories = false,
                AllowsMultipleSelection = false,
                Message = $"Select an image file ({string.Join(", ", fileExtensions)})",
            };

            var result = panel.RunModal();

            if (result == 1)  // NSModalResponse.OK
            {
                return panel.Url?.Path;
            }
            return null;
        });
    }
}

#endif
