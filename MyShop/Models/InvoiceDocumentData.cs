namespace MyShop.Models;

/// <summary>
/// Format-agnostic invoice data transfer object.
/// Decouples PDF/XPS rendering from the Npgsql domain model (CustomerOrder / OrderDetail).
/// </summary>
public sealed class InvoiceDocumentData
{
    // ── Company header ─────────────────────────────────────────────
    // ── Company header ─────────────────────────────────────────────
    public string CompanyName    { get; init; } = "MYSHOP - SNEAKER & FASHION";
    public string CompanyAddress { get; init; } = "123 Street, District 1, HCMC";
    public string CompanyPhone   { get; init; } = "Hotline: 1900 1234";

    // ── Order identity ─────────────────────────────────────────────
    public string          OrderNumber { get; init; } = "";
    public DateTimeOffset OrderDate   { get; init; }

    // ── Customer ───────────────────────────────────────────────────
    public string CustomerName     { get; init; } = "";
    public string CustomerPhone    { get; init; } = "";
    public string ShippingAddress   { get; init; } = "";
    public string OrderType        { get; init; } = "AtStore";  // "AtStore" | "Delivery"

    // ── Line items ─────────────────────────────────────────────────
    public IReadOnlyList<LineItem> LineItems { get; init; } = [];

    // ── Totals ─────────────────────────────────────────────────────
    public decimal Subtotal   { get; init; }
    public decimal TaxRate    { get; init; } = 0.0m;   // e.g. 0.08m = 8% VAT
    public decimal TaxAmount  => Subtotal * TaxRate;
    public decimal GrandTotal => Subtotal + TaxAmount;

    // ── Footer ─────────────────────────────────────────────────────
    public string Notes      { get; init; } = "";
    public string SellerName { get; init; } = "";
}

/// <summary>
/// A single line on the invoice.
/// </summary>
public sealed record LineItem(
    int     Index,
    string  ItemName,
    int     Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
