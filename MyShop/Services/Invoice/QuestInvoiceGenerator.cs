namespace MyShop.Services.Invoice;

using MyShop.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

/// <summary>
/// Unified generator for Invoices using QuestPDF.
/// Supports PDF (cross-platform) and XPS (Windows-optimized).
/// </summary>
public static class QuestInvoiceGenerator
{
    // Brand colors matching the app's purple theme
    private const string Purple    = "#8B52FF";
    private const string DarkGray  = "#1F2937";
    private const string MidGray   = "#6B7280";
    private const string LightGray = "#E5E7EB";
    private const string RowGray   = "#F9FAFB";

    static QuestInvoiceGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates and saves a PDF invoice. Works on all platforms.
    /// </summary>
    public static Task SavePdfAsync(InvoiceDocumentData data, string path, CancellationToken ct = default)
        => Task.Run(() =>
        {
            Document.Create(doc => Compose(doc, data)).GeneratePdf(path);
        }, ct);

    /// <summary>
    /// Generates and saves an XPS invoice.
    /// Note: QuestPDF XPS generation is natively supported on Windows.
    /// </summary>
    public static Task SaveXpsAsync(InvoiceDocumentData data, string path, CancellationToken ct = default)
        => Task.Run(() =>
        {
            // Use runtime check instead of preprocessor for Skia compatibility
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Document.Create(doc => Compose(doc, data)).GenerateXps(path);
            }
            else
            {
                throw new NotSupportedException("XPS generation is only supported on Windows systems.");
            }
        }, ct);

    private static void Compose(IDocumentContainer doc, InvoiceDocumentData d)
    {
        doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));
            page.Header().Element(c => RenderHeader(c, d));
            page.Content().Element(c => RenderBody(c, d));
            page.Footer().Element(c => RenderFooter(c, d));
        });
    }

    // ── Header ─────────────────────────────────────────────────────

    private static void RenderHeader(IContainer c, InvoiceDocumentData d)
    {
        c.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c1 =>
                {
                    c1.Item().Text(d.CompanyName).Bold().FontSize(18).FontColor(Purple);
                    c1.Item().Text(d.CompanyAddress).FontSize(8).FontColor(MidGray);
                    c1.Item().Text(d.CompanyPhone).FontSize(8).FontColor(MidGray);
                });

                row.ConstantItem(150).AlignRight().Column(c2 =>
                {
                    c2.Item().AlignRight().Text("INVOICE").Bold().FontSize(24).FontColor(DarkGray);
                    c2.Item().AlignRight().Text(d.OrderNumber).FontSize(12).FontColor(Purple);
                    c2.Item().AlignRight().Text(d.OrderDate.ToString("dd/MM/yyyy")).FontSize(8).FontColor(MidGray);
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(Purple);
        });
    }

    // ── Body ────────────────────────────────────────────────────────

    private static void RenderBody(IContainer c, InvoiceDocumentData d)
    {
        c.PaddingTop(12).Column(col =>
        {
            // Customer + Order Type info row
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c1 =>
                {
                    c1.Item().Text("BILL TO").FontSize(8).FontColor(MidGray).SemiBold();
                    c1.Item().PaddingTop(2).Text(d.CustomerName).Bold().FontColor(DarkGray);
                    c1.Item().Text(d.CustomerPhone).FontSize(8).FontColor(MidGray);
                    if (!string.IsNullOrWhiteSpace(d.ShippingAddress) && d.ShippingAddress != "N/A")
                        c1.Item().Text(d.ShippingAddress).FontSize(8).FontColor(MidGray);
                });

                row.RelativeItem().Column(c2 =>
                {
                    c2.Item().Text("ORDER TYPE").FontSize(8).FontColor(MidGray).SemiBold();
                    c2.Item().PaddingTop(2).Text(d.OrderType).FontColor(DarkGray);
                    if (!string.IsNullOrWhiteSpace(d.Notes))
                    {
                        c2.Item().PaddingTop(8).Text("NOTES").FontSize(8).FontColor(MidGray).SemiBold();
                        c2.Item().Text(d.Notes).FontSize(8).FontColor(MidGray);
                    }
                });
            });

            col.Item().PaddingVertical(14);

            // Line items table
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(def =>
                {
                    def.RelativeColumn(1);   // #
                    def.RelativeColumn(3);   // Product
                    def.ConstantColumn(50); // Qty
                    def.ConstantColumn(90); // Unit Price
                    def.ConstantColumn(90); // Total
                });

                // Header row — inline to avoid type-name issues
                table.Header(header =>
                {
                    header.Cell().Background(RowGray).Padding(6)
                        .Text("#").FontSize(8).Bold().FontColor(MidGray);
                    header.Cell().Background(RowGray).Padding(6)
                        .Text("Product").FontSize(8).Bold().FontColor(MidGray);
                    header.Cell().Background(RowGray).Padding(6).PaddingRight(6)
                        .AlignRight().Text("Qty").FontSize(8).Bold().FontColor(MidGray);
                    header.Cell().Background(RowGray).Padding(6).PaddingRight(6)
                        .AlignRight().Text("Unit Price").FontSize(8).Bold().FontColor(MidGray);
                    header.Cell().Background(RowGray).Padding(6).PaddingRight(6)
                        .AlignRight().Text("Total").FontSize(8).Bold().FontColor(MidGray);
                });

                // Data rows
                foreach (var item in d.LineItems)
                {
                    table.Cell().BorderBottom(1).BorderColor(RowGray).Padding(6)
                        .Text(item.Index.ToString()).FontSize(9).FontColor(DarkGray);
                    table.Cell().BorderBottom(1).BorderColor(RowGray).Padding(6)
                        .Text(item.ItemName ?? "").FontSize(9).FontColor(DarkGray);
                    table.Cell().BorderBottom(1).BorderColor(RowGray).Padding(6).PaddingRight(6)
                        .AlignRight().Text(item.Quantity.ToString()).FontSize(9).FontColor(DarkGray);
                    table.Cell().BorderBottom(1).BorderColor(RowGray).Padding(6).PaddingRight(6)
                        .AlignRight().Text($"${item.UnitPrice:N2}").FontSize(9).FontColor(DarkGray);
                    table.Cell().BorderBottom(1).BorderColor(RowGray).Padding(6).PaddingRight(6)
                        .AlignRight().Text($"${item.LineTotal:N2}").FontSize(9).Bold().FontColor(DarkGray);
                }
            });

            col.Item().PaddingTop(10);

            // Totals block — right-aligned
            col.Item().AlignRight().Column(totals =>
            {
                totals.Item().Row(r =>
                {
                    r.ConstantItem(130).AlignRight().Text("Subtotal:").FontColor(MidGray);
                    r.ConstantItem(100).AlignRight().Text($"${d.Subtotal:N2}").FontColor(DarkGray);
                });

                if (d.TaxRate > 0)
                {
                    totals.Item().Row(r =>
                    {
                        r.ConstantItem(130).AlignRight().Text($"Tax ({(d.TaxRate * 100):0}%):").FontColor(MidGray);
                        r.ConstantItem(100).AlignRight().Text($"${d.TaxAmount:N2}").FontColor(MidGray);
                    });
                }

                totals.Item().PaddingTop(4).Row(r =>
                {
                    r.ConstantItem(130).AlignRight().Text("GRAND TOTAL:").Bold().FontSize(12).FontColor(DarkGray);
                    r.ConstantItem(100).AlignRight().Text($"${d.GrandTotal:N2}").Bold().FontSize(12).FontColor(Purple);
                });
            });
        });
    }

    // ── Footer ──────────────────────────────────────────────────────

    private static void RenderFooter(IContainer c, InvoiceDocumentData d)
    {
        c.Column(col =>
        {
            col.Item().PaddingTop(6).LineHorizontal(1).LineColor(LightGray);
            col.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text($"Thank you for shopping at {d.CompanyName}!").FontSize(8).FontColor(MidGray);
                row.ConstantItem(180).AlignRight().Text($"Seller: {d.SellerName}").FontSize(8).FontColor(MidGray);
            });
        });
    }
}
