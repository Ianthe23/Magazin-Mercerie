using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace magazin_mercerie.ViewModels.Converters
{
    public class FirstLetterConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return str[0].ToString().ToUpperInvariant();
            }
            return "?";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 