<<<<<<< P6_Screenings_and_Checkout
using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MovieApp.UI.Converters;

/// <summary>
/// Maps a string URL to a <see cref="BitmapImage"/>; returns null for null/empty/invalid URLs
/// so that <c>Image.Source</c> does not throw on empty strings.
/// </summary>
public sealed class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return new BitmapImage(uri);
=======
﻿namespace MovieApp.UI.Converters;

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

public class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string url && !string.IsNullOrWhiteSpace(url))
        {
            try
            {
                return new BitmapImage(new Uri(url));
            }
            catch
            {
                return null;
            }
        }
        return null;
>>>>>>> main
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
<<<<<<< P6_Screenings_and_Checkout
}
=======
}
>>>>>>> main
