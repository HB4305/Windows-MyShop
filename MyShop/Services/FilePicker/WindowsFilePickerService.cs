namespace MyShop.Services.FilePicker;

#if WINDOWS || NET10_0_DESKTOP || HAS_UNO_SKIA

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Storage.Pickers;

/// <summary>
/// Final stable implementation of IFilePickerService using Task.Run 
/// to ensure the UI thread is never blocked during picker invocation.
/// </summary>
internal sealed class WindowsFilePickerService : IFilePickerService
{
    public async Task<string?> PickSavePathAsync(string defaultFileName, string fileTypeLabel, string fileExtension)
    {
        System.Console.WriteLine($"[FilePicker] Invoking picker in background for: {defaultFileName}");

        try
        {
            // We use Task.Run to decouple the Win32 dialog from the main UI thread.
            // This has proven to be the most stable way to prevent "Not Responding" hangs in Skia.
            var result = await Task.Run(async () =>
            {
                try
                {
                    // Delay to let the previous dialog finish closing animations
                    await Task.Delay(400);

                    var picker = new FileSavePicker();
                    var ext = fileExtension.StartsWith('.') ? fileExtension : $".{fileExtension}";
                    picker.FileTypeChoices.Add(fileTypeLabel, new List<string> { ext });
                    picker.SuggestedFileName = Path.GetFileNameWithoutExtension(defaultFileName);

#if WINDOWS && !HAS_UNO
                    // Native WinUI 3 initialization (not used in Skia but kept for completeness)
                    if (App.MainWindow != null)
                    {
                        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                    }
#endif

                    var file = await picker.PickSaveFileAsync();
                    return file?.Path;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[FilePicker] Thread Exception: {ex.Message}");
                    return null;
                }
            });

            System.Console.WriteLine($"[FilePicker] Picker returned: {result ?? "Cancelled"}");
            return result;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[FilePicker] Global Exception: {ex.Message}");
            return null;
        }
    }
}

#endif
