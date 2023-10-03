using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions;

public static partial class StringExtensions
{
    public static double? ToDouble(this string? s, NumberStyles style = Styles.Float, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        try
        {
            return double.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("double", s, e);
        }
    }

#if NETCOREAPP
    public static double? ToDouble(this ReadOnlySpan<char> s, NumberStyles style = Styles.Float, IFormatProvider? provider = null)
    {
        if (s.IsEmpty)
        {
            return null;
        }

        try
        {
            return double.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("double", s, e);
        }
    }
#endif

    public static double? ToDoubleSafe(this string? s, NumberStyles style = Styles.Float, IFormatProvider? provider = null) =>
        double.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
    public static double? ToDoubleSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Float, IFormatProvider? provider = null) =>
        double.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif
}
