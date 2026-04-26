using ClosedXML.Excel;
using MyShop.Models;

namespace MyShop.Services;

/// <summary>
/// Represents one parsed row from an import file, including any validation errors.
/// </summary>
public class ImportRow
{
    public int RowNumber { get; set; }

    // Raw parsed values
    public string RawName { get; set; } = string.Empty;
    public string RawCategory { get; set; } = string.Empty;
    public string RawCostPrice { get; set; } = string.Empty;
    public string RawSellingPrice { get; set; } = string.Empty;
    public string RawStock { get; set; } = string.Empty;
    public string RawLowStockThreshold { get; set; } = string.Empty;
    public string RawDescription { get; set; } = string.Empty;

    // Parsed values (null = parse failed)
    public string? Name { get; set; }
    public string? CategoryName { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public int? StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public string? Description { get; set; }

    public List<string> Errors { get; set; } = new();
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Convert a valid ImportRow to a SportItem ready for DB insert.
    /// Category resolution (name → id) is done by the caller.
    /// </summary>
    public SportItem ToSportItem(int categoryId = 0) => new()
    {
        Name = Name ?? string.Empty,
        CategoryId = categoryId,
        CostPrice = CostPrice,
        SellingPrice = SellingPrice,
        StockQuantity = StockQuantity ?? 0,
        LowStockThreshold = LowStockThreshold ?? 5,
        Description = Description,
        ImageUrls = new List<string>()
    };
}

/// <summary>
/// Parses an Excel (.xlsx) file into ImportRow list with per-row validation.
/// Expects columns in order: Name | Category | Cost Price | Selling Price | Stock | Low Stock | Description
/// Row 1 is treated as a header and is skipped.
/// </summary>
public class ExcelImportService
{
    // Expected header column names (case-insensitive match)
    private static readonly string[] ExpectedHeaders =
    {
        "name", "category", "selling price",
        "low stock threshold", "description"
    };

    /// <summary>
    /// Parse the stream of an .xlsx file. Returns parsed rows.
    /// Expects columns: 1:Name | 2:Category | 3:Selling Price | 4:Low Stock | 5:Description
    /// </summary>
    public List<ImportRow> ParseExcel(Stream stream)
    {
        var rows = new List<ImportRow>();

        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();

        int dataStartRow = 2;
        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int r = dataStartRow; r <= lastRow; r++)
        {
            var row = ws.Row(r);
            if (ws.Row(r).IsEmpty()) continue;

            var importRow = new ImportRow { RowNumber = r };

            importRow.RawName = GetCellString(row, 1);
            importRow.RawCategory = GetCellString(row, 2);
            importRow.RawSellingPrice = GetCellString(row, 3);
            importRow.RawLowStockThreshold = GetCellString(row, 4);
            importRow.RawDescription = GetCellString(row, 5);

            // Cost and Stock default to 0
            importRow.CostPrice = 0;
            importRow.StockQuantity = 0;

            // --- Validate Name ---
            if (string.IsNullOrWhiteSpace(importRow.RawName))
                importRow.Errors.Add("Name is required");
            else
                importRow.Name = importRow.RawName.Trim();

            // --- Category ---
            importRow.CategoryName = string.IsNullOrWhiteSpace(importRow.RawCategory)
                ? null : importRow.RawCategory.Trim();

            // --- Selling Price (required) ---
            if (string.IsNullOrWhiteSpace(importRow.RawSellingPrice))
            {
                importRow.Errors.Add("Selling Price is required");
            }
            else if (decimal.TryParse(importRow.RawSellingPrice, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var sp))
            {
                if (sp < 0)
                    importRow.Errors.Add("Selling Price cannot be negative");
                else
                    importRow.SellingPrice = sp;
            }
            else
                importRow.Errors.Add($"Invalid Selling Price: '{importRow.RawSellingPrice}'");

            // --- Low Stock Threshold ---
            if (!string.IsNullOrWhiteSpace(importRow.RawLowStockThreshold))
            {
                if (int.TryParse(importRow.RawLowStockThreshold.Trim(), out var lst) && lst >= 0)
                    importRow.LowStockThreshold = lst;
                else
                    importRow.Errors.Add($"Invalid Low Stock Threshold: '{importRow.RawLowStockThreshold}'");
            }
            else
                importRow.LowStockThreshold = 5;

            // --- Description ---
            importRow.Description = string.IsNullOrWhiteSpace(importRow.RawDescription)
                ? null : importRow.RawDescription.Trim();

            rows.Add(importRow);
        }

        return rows;
    }

    /// <summary>
    /// Generate a template .xlsx file in memory for the user to download.
    /// </summary>
    public byte[] GenerateTemplate()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Products");

        // Header row
        ws.Cell(1, 1).Value = "Name *";
        ws.Cell(1, 2).Value = "Category";
        ws.Cell(1, 3).Value = "Selling Price *";
        ws.Cell(1, 4).Value = "Low Stock Threshold";
        ws.Cell(1, 5).Value = "Description";

        // Style header
        var headerRow = ws.Range(1, 1, 1, 5);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#8B52FF");
        headerRow.Style.Font.FontColor = XLColor.White;

        // Sample data row
        ws.Cell(2, 1).Value = "Nike Air Zoom";
        ws.Cell(2, 2).Value = "Shoes";
        ws.Cell(2, 3).Value = 99.99;
        ws.Cell(2, 4).Value = 10;
        ws.Cell(2, 5).Value = "Sample product description";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string GetCellString(IXLRow row, int col)
        => row.Cell(col).GetString().Trim();
}
