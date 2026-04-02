namespace MyShop.Models;

/// <summary>
/// Một dòng hiển thị trong danh sách sản phẩ — gắn thêm tên danh mục từ bảng categories.
/// </summary>
public sealed class SportItemListRow
{
    public required SportItem Item { get; init; }
    public string CategoryName { get; init; } = "—";
}
