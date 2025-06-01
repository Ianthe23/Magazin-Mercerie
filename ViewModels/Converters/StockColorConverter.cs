using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace magazin_mercerie.ViewModels.Converters
{
    public class StockColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal decimalStock)
            {
                var stock = (int)decimalStock;
                if (stock <= 0)
                    return new SolidColorBrush(Colors.Red); // Out of stock - bright red
                else if (stock <= 5)
                    return new SolidColorBrush(Colors.IndianRed); // Low stock - red
                else if (stock <= 15)
                    return new SolidColorBrush(Colors.Orange); // Medium stock - orange
                else
                    return new SolidColorBrush(Colors.LimeGreen); // High stock - green
            }
            else if (value is int stock)
            {
                if (stock <= 0)
                    return new SolidColorBrush(Colors.Red); // Out of stock - bright red
                else if (stock <= 5)
                    return new SolidColorBrush(Colors.IndianRed); // Low stock - red
                else if (stock <= 15)
                    return new SolidColorBrush(Colors.Orange); // Medium stock - orange
                else
                    return new SolidColorBrush(Colors.LimeGreen); // High stock - green
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 