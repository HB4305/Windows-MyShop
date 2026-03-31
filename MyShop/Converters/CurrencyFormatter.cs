using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class CurrencyFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d)
        {
            return d.ToString("N0") + "đ";
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
