using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MovieApp.UI.Converters;

public class NullableIntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility visibility && visibility == Visibility.Visible ? 1 : 0;
    }
}
