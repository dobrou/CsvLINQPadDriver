using System;
using System.Globalization;

namespace CsvLINQPadDriver.Extensions;

internal static class EnumExtensions
{
    public static Func<int, string> GetFormatFunc<T>(this T value) where T: Enum
    {
        var name = Enum.GetName(typeof(T), value);
        if (name is null)
        {
            throw new IndexOutOfRangeException($"Unknown {typeof(T).Name} {value}");
        }

        var format = $"{name[..^1]}{{0}}";
        var startIndex = name[^1] == '0' ? 0 : 1;

        return i => string.Format(CultureInfo.InvariantCulture, format, i + startIndex);
    }
}
