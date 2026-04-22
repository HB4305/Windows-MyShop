namespace MyShop.Services;

using MyShop.Services.Invoice;

using MyShop.Models;

/// <summary>
/// Platform-aware entry point for invoice export.
/// XPS / Print are Windows-only; PDF uses QuestPDF on all platforms.
/// </summary>
public sealed class InvoiceService : IInvoiceService
{
    public bool SupportsXps =>
#if WINDOWS
        true;
#else
        false;
#endif

    public bool SupportsPdf => true;

    public async Task PrintAsync(Models.InvoiceDocumentData data, CancellationToken ct = default)
    {
#if WINDOWS
        // For printing, we generate a temporary XPS and send it to the default printer
        var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Invoice_{System.Guid.NewGuid()}.xps");
        try
        {
            await QuestInvoiceGenerator.SaveXpsAsync(data, tempPath, ct);
            // On Windows, 'print' verb on an XPS file usually opens the print dialog or sends to default printer
            var psi = new System.Diagnostics.ProcessStartInfo(tempPath)
            {
                Verb = "print",
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to start print job: {ex.Message}", ex);
        }
#else
        throw new NotSupportedException("Print is only available on Windows.");
#endif
    }

    public async Task SavePdfAsync(Models.InvoiceDocumentData data, string path, CancellationToken ct = default)
    {
        await QuestInvoiceGenerator.SavePdfAsync(data, path, ct);
    }

    public async Task SaveXpsAsync(Models.InvoiceDocumentData data, string path, CancellationToken ct = default)
    {
#if WINDOWS
        await QuestInvoiceGenerator.SaveXpsAsync(data, path, ct);
#else
        // On non-Windows, QuestPDF doesn't support native XPS.
        // We could generate a PDF and rename it, but it's better to be explicit.
        throw new NotSupportedException("XPS export is only available on Windows.");
#endif
    }
}
