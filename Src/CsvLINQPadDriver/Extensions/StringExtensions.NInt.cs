#if NET5_0_OR_GREATER
using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions;

public static partial class StringExtensions
{
    public static nint? ToNInt(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        try
        {
            return nint.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("nint", s, e);
        }
    }

#if NET6_0_OR_GREATER
    public static nint? ToNInt(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
    {
        if (s.IsEmpty)
        {
            return null;
        }

        try
        {
            return nint.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("nint", s, e);
        }
    }
#endif

    public static nuint? ToNUInt(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        try
        {
            return nuint.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("nuint", s, e);
        }
    }

#if NET6_0_OR_GREATER
    public static nuint? ToNUInt(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
    {
        if (s.IsEmpty)
        {
            return null;
        }

        try
        {
            return nuint.Parse(s, style, provider.ResolveFormatProvider());
        }
        catch (Exception e) when (e.CanBeHandled())
        {
            throw ConvertException.Create("nuint", s, e);
        }
    }
#endif

    public static nint? ToNIntSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
        nint.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NET6_0_OR_GREATER
    public static nint? ToNIntSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
        nint.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif

    public static nuint? ToNUIntSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
        nuint.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NET6_0_OR_GREATER
    public static nuint? ToNUIntSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
        nuint.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif
}
#endif
