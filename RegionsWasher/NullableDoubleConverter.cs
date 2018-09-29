using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RegionsWasher
{
    internal class NullableDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (double.TryParse(text, out double result))
            {
                return result;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
