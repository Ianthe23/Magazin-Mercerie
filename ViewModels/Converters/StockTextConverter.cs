using System;
using System.Globalization;
using Avalonia.Data.Converters;
using magazin_mercerie.Service;
using Microsoft.Extensions.DependencyInjection;

namespace magazin_mercerie.ViewModels.Converters
{
    public class StockTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal decimalStock)
            {
                var stock = (int)decimalStock;
                if (stock <= 0)
                    return "Out of Stock";
                else if (stock <= 5)
                    return "Low Stock";
                else if (stock <= 15)
                    return "Medium Stock";
                else
                    return "In Stock";
            }
            else if (value is int stock)
            {
                if (stock <= 0)
                    return "Out of Stock";
                else if (stock <= 5)
                    return "Low Stock";
                else if (stock <= 15)
                    return "Medium Stock";
                else
                    return "In Stock";
            }
            return "Unknown";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StockDisplayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return "Out of stock";
                
                int stock = 0;
                if (value is decimal decimalStock)
                {
                    stock = (int)decimalStock;
                }
                else if (value is int intStock)
                {
                    stock = intStock;
                }
                else if (value is double doubleStock)
                {
                    stock = (int)doubleStock;
                }
                else if (value is float floatStock)
                {
                    stock = (int)floatStock;
                }
                else if (int.TryParse(value.ToString(), out var parsedStock))
                {
                    stock = parsedStock;
                }
                
                if (stock <= 0)
                    return "Out of stock";
                else if (stock <= 5)
                    return "Low stock";
                else
                    return "In stock";
            }
            catch
            {
                return "Out of stock";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StockToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return false;
                
                int stock = 0;
                if (value is decimal decimalStock)
                {
                    stock = (int)decimalStock;
                }
                else if (value is int intStock)
                {
                    stock = intStock;
                }
                else if (value is double doubleStock)
                {
                    stock = (int)doubleStock;
                }
                else if (value is float floatStock)
                {
                    stock = (int)floatStock;
                }
                else if (int.TryParse(value.ToString(), out var parsedStock))
                {
                    stock = parsedStock;
                }
                
                return stock > 0; // Returns true if stock > 0, false if stock <= 0
            }
            catch
            {
                return false;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PriceFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return "$0.00";
                
                if (value is decimal price)
                {
                    return $"${price:F2}";
                }
                
                if (value is double doublePrice)
                {
                    return $"${doublePrice:F2}";
                }
                
                if (value is float floatPrice)
                {
                    return $"${floatPrice:F2}";
                }
                
                if (value is int intPrice)
                {
                    return $"${intPrice:F2}";
                }
                
                // Try to convert whatever we got to decimal
                if (decimal.TryParse(value.ToString(), out var parsedPrice))
                {
                    return $"${parsedPrice:F2}";
                }
                
                return "$0.00";
            }
            catch
            {
                return "$0.00";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StockFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return "0";
                
                if (value is decimal stock)
                {
                    return $"{stock:F0}";
                }
                
                if (value is double doubleStock)
                {
                    return $"{doubleStock:F0}";
                }
                
                if (value is float floatStock)
                {
                    return $"{floatStock:F0}";
                }
                
                if (value is int intStock)
                {
                    return intStock.ToString();
                }
                
                // Try to convert whatever we got to decimal
                if (decimal.TryParse(value.ToString(), out var parsedStock))
                {
                    return $"{parsedStock:F0}";
                }
                
                return "0";
            }
            catch
            {
                return "0";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmployeeOnlineStatusConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is Angajat employee)
                {
                    var userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
                    if (userSessionService != null)
                    {
                        return userSessionService.IsEmployeeOnline(employee.Id);
                    }
                }
                return false; // Default to offline if we can't determine
            }
            catch
            {
                return false; // Default to offline on error
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmployeeStatusTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is Angajat employee)
                {
                    var userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
                    if (userSessionService != null)
                    {
                        return userSessionService.IsEmployeeOnline(employee.Id) ? "Online" : "Offline";
                    }
                }
                return "Offline"; // Default to offline if we can't determine
            }
            catch
            {
                return "Offline"; // Default to offline on error
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmployeeStatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is Angajat employee)
                {
                    var userSessionService = App.ServiceProvider?.GetService<IUserSessionService>();
                    if (userSessionService != null)
                    {
                        bool isOnline = userSessionService.IsEmployeeOnline(employee.Id);
                        return isOnline ? 
                            new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.LimeGreen) : 
                            new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray);
                    }
                }
                return new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray); // Default to gray (offline)
            }
            catch
            {
                return new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray); // Default to gray on error
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 