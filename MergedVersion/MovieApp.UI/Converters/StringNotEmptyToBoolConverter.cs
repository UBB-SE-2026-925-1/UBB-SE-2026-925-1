using Microsoft.UI.Xaml.Data;
using System;

namespace MovieApp.UI.Converters;

public class StringNotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str)
        {
            return !string.IsNullOrEmpty(str);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
