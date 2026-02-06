using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Ogur.Terraria.Manager.Devexpress.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "connected" => new SolidColorBrush(Color.FromRgb(0, 255, 0)),      // Zielony
                "connecting..." => new SolidColorBrush(Color.FromRgb(255, 255, 0)), // Żółty
                "disconnected" => new SolidColorBrush(Color.FromRgb(255, 68, 68)),  // Czerwony
                "connection failed" => new SolidColorBrush(Color.FromRgb(255, 68, 68)), // Czerwony
                _ => new SolidColorBrush(Colors.White)
            };
        }
        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}