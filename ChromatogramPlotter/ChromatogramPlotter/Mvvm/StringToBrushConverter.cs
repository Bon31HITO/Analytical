using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChromatogramPlotter.Mvvm
{
    class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString)
            {
                try
                {
                    return (SolidColorBrush)new BrushConverter().ConvertFromString(colorString);
                }
                catch
                {
                    return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}