using System;
using System.Globalization;
using System.Windows.Data;

using CsvLINQPadDriver.Extensions;

namespace CsvLINQPadDriver.Wpf.Converters
{
    [ValueConversion(typeof(string), typeof(int))]
    [ValueConversion(typeof(int), typeof(string))]
    internal sealed class SkipLeadingRowsCountConverter : IValueConverter
    {
        private const int DefaultCount = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;
            return intValue == DefaultCount
                        ? string.Empty
                        : intValue.ToString(StringExtensions.DefaultFormatProvider);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            ((string?)value).ToInt() ?? DefaultCount;
    }
}
