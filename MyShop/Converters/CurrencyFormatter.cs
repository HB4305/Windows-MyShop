using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

/// <summary>
/// Hiển thị đúng giá trị trong DB, chỉ thêm ký hiệu $ (không quy đổi).
/// </summary>
public class CurrencyFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is null)
            return "$0.00";
        if (value is not decimal d)
            return "$0.00";
        return "$" + d.ToString("N2", CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
