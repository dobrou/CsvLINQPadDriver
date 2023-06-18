#if NET6_0_OR_GREATER
using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static DateOnly? ToDateOnly(this string? s, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateOnly.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateOnly", s, e);
            }
        }

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateOnly.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateOnly", s, e);
            }
        }

        public static DateOnly? ToDateOnly(this string? s, string format, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateOnly.ParseExact(s, format, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateOnly", s, e);
            }
        }

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateOnly.ParseExact(s, format, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateOnly", s, e);
            }
        }

        public static DateOnly? ToDateOnly(this string? s, string[] formats, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateOnly.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateOnly", s, e);
            }
        }

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> s, string[] formats, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateOnly.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateOnly", s, e);
            }
        }

        public static DateOnly? ToDateOnlySafe(this string? s, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null) =>
            DateOnly.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static DateOnly? ToDateOnlySafe(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null) =>
            DateOnly.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static DateOnly? ToDateOnlySafe(this string? s, string format, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null) =>
            DateOnly.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static DateOnly? ToDateOnlySafe(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null) =>
            DateOnly.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static DateOnly? ToDateOnlySafe(this string? s, string[] formats, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null) =>
            DateOnly.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static DateOnly? ToDateOnlySafe(this ReadOnlySpan<char> s, string[] formats, DateTimeStyles style = Styles.DateOnly, IFormatProvider? provider = null) =>
            DateOnly.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;
    }
}
#endif

