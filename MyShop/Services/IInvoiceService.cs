using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Abstraction for invoice export (PDF, XPS, Print).
/// XPS/Print are Windows-only; PDF works on all platforms via QuestPDF.
/// </summary>
public interface IInvoiceService
{
    /// <summary>True on Windows, false on other platforms.</summary>
    bool SupportsXps { get; }

    /// <summary>True on all platforms (QuestPDF).</summary>
    bool SupportsPdf { get; }

    /// <summary>
    /// Opens the native Windows Print dialog for the given invoice.
    /// Throws <see cref="NotSupportedException"/> on non-Windows.
    /// </summary>
    Task PrintAsync(InvoiceDocumentData data, CancellationToken ct = default);

    /// <summary>
    /// Saves the invoice as a PDF file at <paramref name="path"/>.
    /// Works on all platforms.
    /// </summary>
    Task SavePdfAsync(Models.InvoiceDocumentData data, string path, CancellationToken ct = default);

    /// <summary>
    /// Saves the invoice as an XPS file at <paramref name="path"/>.
    /// Throws <see cref="NotSupportedException"/> on non-Windows.
    /// </summary>
    Task SaveXpsAsync(Models.InvoiceDocumentData data, string path, CancellationToken ct = default);
}
