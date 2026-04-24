namespace MyShop.Models;

/// <summary>
/// A row displayed in the product list - includes the category name from the categories table.
/// </summary>
public sealed class SportItemListRow
{
    public required SportItem Item { get; init; }
    public string CategoryName { get; init; } = "—";
}
