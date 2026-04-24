using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using MyShop.Models;
using MyShop.Services;
using MyShop.Views.Dialogs;

namespace MyShop.ViewModels;

/// <summary>
/// Orchestrates the invoice export flow:
/// 1. Show format picker dialog (PDF / XPS / Print)
/// 2. Dispatch to the correct IInvoiceService method
/// </summary>
public partial class InvoiceDialogViewModel : ObservableObject
{
    private readonly IInvoiceService _invoiceService;
    private readonly IFilePickerService _filePicker;
    private readonly XamlRoot _xamlRoot;

    public CustomerOrder Order { get; }
    public System.Collections.ObjectModel.ObservableCollection<OrderDetail> Details { get; }

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private string _statusMessage = "";

    public InvoiceDialogViewModel(
        CustomerOrder order,
        System.Collections.ObjectModel.ObservableCollection<OrderDetail> details,
        IInvoiceService invoiceService,
        IFilePickerService filePicker,
        XamlRoot xamlRoot)
    {
        Order = order;
        Details = details;
        _invoiceService = invoiceService;
        _filePicker = filePicker;
        _xamlRoot = xamlRoot;
    }

    /// <summary>
    /// Maps the domain model (CustomerOrder + OrderDetail[]) to the format-agnostic DTO.
    /// </summary>
    public InvoiceDocumentData ToDocumentData()
    {
        var items = Details.Select((d, i) => new LineItem(
            Index:     i + 1,
            ItemName:  d.ItemName ?? "",
            Quantity:  d.Quantity,
            UnitPrice: d.UnitPrice,
            LineTotal: d.Quantity * d.UnitPrice
        )).ToList();

        return new InvoiceDocumentData
        {
            OrderNumber     = $"#ATH-{Order.Id:D5}",
            OrderDate       = Order.CreatedAt ?? DateTimeOffset.Now,
            CustomerName    = Order.CustomerName,
            CustomerPhone   = Order.CustomerPhone,
            ShippingAddress = Order.ShippingAddress ?? "N/A",
            OrderType       = Order.OrderType ?? "AtStore",
            LineItems       = items,
            Subtotal        = items.Sum(x => x.LineTotal),
            TaxRate         = 0.0m,   
            Notes           = Order.Notes ?? "",
            SellerName      = Order.SellerName ?? "Admin",
            CompanyName     = "MYSHOP - SNEAKER & FASHION",
            CompanyAddress  = "123 Street, District 1, Ho Chi Minh City",
            CompanyPhone    = "Hotline: 1900 1234"
        };
    }

    /// <summary>
    /// Shows the format picker, then dispatches to the selected export path.
    /// Returns a human-readable result message.
    /// </summary>
    public async Task<string> ExportAsync()
    {
        var dialog = new ExportFormatDialog($"#ATH-{Order.Id:D5}")
        {
            XamlRoot = _xamlRoot
        };

        // User taps a format card to select it, then clicks Export or Cancel (manual buttons).
        await dialog.ShowAsync();
        var result = dialog.CustomResult;

        System.Console.WriteLine($"[InvoiceVM] Custom result: {result}, Selected Mode: {dialog.SelectedMode}");

        // If user cancelled, ensure we don't proceed
        if (result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            return "Export cancelled.";

        var selectedMode = dialog.SelectedMode;
        var data = ToDocumentData();

        // Important: Return control to UI loop briefly to ensure dialog is fully gone
        await Task.Yield();

        return selectedMode switch
        {
            ExportFormatMode.Pdf   => await ExportPdfAsync(data),
            ExportFormatMode.Xps   => await ExportXpsAsync(data),
            _                      => "No format selected."
        };
    }

    private async Task<string> ExportPdfAsync(InvoiceDocumentData data)
    {
        IsExporting = true;
        StatusMessage = "Generating PDF…";
        try
        {
            var fileName = $"Invoice-ATH-{Order.Id:D5}.pdf";
            var path = await _filePicker.PickSavePathAsync(fileName, "PDF Document", ".pdf");
            if (string.IsNullOrEmpty(path)) return "PDF export cancelled.";

            await _invoiceService.SavePdfAsync(data, path);
            return $"Invoice saved:\n{path}";
        }
        catch (Exception ex)
        {
            return $"PDF export failed:\n{ex.Message}";
        }
        finally
        {
            IsExporting = false;
            StatusMessage = "";
        }
    }

    private async Task<string> ExportXpsAsync(InvoiceDocumentData data)
    {
        IsExporting = true;
        StatusMessage = "Exporting XPS…";
        try
        {
            var fileName = $"Invoice-ATH-{Order.Id:D5}.xps";
            var path = await _filePicker.PickSavePathAsync(fileName, "XPS Document", ".xps");
            if (string.IsNullOrEmpty(path)) return "XPS export cancelled.";

            await _invoiceService.SaveXpsAsync(data, path);
            return $"Invoice saved:\n{path}";
        }
        catch (Exception ex)
        {
            return $"XPS export failed:\n{ex.Message}";
        }
        finally
        {
            IsExporting = false;
            StatusMessage = "";
        }
    }
}