using System;
using System.Globalization;
using System.Windows.Data;

using CsvLINQPadDriver.Extensions;

namespace CsvLINQPadDriver.Wpf.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    internal class RemoveHotKeyCharValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            ((string)value).ReplaceHotKeyChar();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
