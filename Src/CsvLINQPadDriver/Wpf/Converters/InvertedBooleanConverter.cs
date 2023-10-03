using System;
using System.Globalization;
using System.Windows.Data;

namespace CsvLINQPadDriver.Wpf.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    internal sealed class InvertedBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            !(bool)value!;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
