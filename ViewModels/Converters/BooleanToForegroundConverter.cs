using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace magazin_mercerie.ViewModels.Converters
{
    public class BooleanToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return new SolidColorBrush(Colors.White);
            }
            return new SolidColorBrush(Color.Parse("#ffffff"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 