using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions;

public static partial class StringExtensions
{
    public static sbyte? ToSByte(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        try
        {
            return sbyte.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("sbyte", s, e);
        }
    }

#if NETCOREAPP
    public static sbyte? ToSByte(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer,
        IFormatProvider? provider = null)
    {
        if (s.IsEmpty)
        {
            return null;
        }

        try
        {
            return sbyte.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("sbyte", s, e);
        }
    }
#endif

    public static byte? ToByte(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        try
        {
            return byte.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("byte", s, e);
        }
    }

#if NETCOREAPP
    public static byte? ToByte(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
    {
        if (s.IsEmpty)
        {
            return null;
        }

        try
        {
            return byte.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("byte", s, e);
        }
    }
#endif

    public static sbyte? ToSByteSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
        sbyte.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
    public static sbyte? ToSByteSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
        sbyte.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif

    public static byte? ToByteSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
        byte.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
    public static byte? ToByteSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
        byte.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif
}
