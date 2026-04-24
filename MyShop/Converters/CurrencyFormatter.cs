using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

/// <summary>
/// Displays the value from DB correctly with a $ symbol (no conversion).
/// </summary>
public class CurrencyFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d)
        {
            return $"${d:N2}";
        }

        if (value is double db)
        {
            return $"${db:N2}";
        }

        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
