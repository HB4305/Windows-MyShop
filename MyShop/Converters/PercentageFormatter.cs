using System;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Converters;

public class PercentageFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d)
        {
            return $"{d:F2}%";
        }

        if (value is double db)
        {
            return $"{db:F2}%";
        }

        if (value is float f)
        {
            return $"{f:F2}%";
        }

        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
