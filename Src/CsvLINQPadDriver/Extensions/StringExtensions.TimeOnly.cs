#if NET6_0_OR_GREATER
using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static TimeOnly? ToTimeOnly(this string? s, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return TimeOnly.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeOnly", s, e);
            }
        }

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return TimeOnly.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeOnly", s, e);
            }
        }

        public static TimeOnly? ToTimeOnly(this string? s, string format, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return TimeOnly.ParseExact(s, format, provider.ResolveFormatProvider(), style);

            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeOnly", s, e);
            }
        }

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return TimeOnly.ParseExact(s, format, provider.ResolveFormatProvider(), style);

            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeOnly", s, e);
            }
        }

        public static TimeOnly? ToTimeOnly(this string? s, string[] formats, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return TimeOnly.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeOnly", s, e);
            }
        }

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> s, string[] formats, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return TimeOnly.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeOnly", s, e);
            }
        }

        public static TimeOnly? ToTimeOnlySafe(this string? s, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null) =>
            TimeOnly.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static TimeOnly? ToTimeOnlySafe(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null) =>
            TimeOnly.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static TimeOnly? ToTimeOnlySafe(this string? s, string format, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null) =>
            TimeOnly.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static TimeOnly? ToTimeOnlySafe(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null) =>
            TimeOnly.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static TimeOnly? ToTimeOnlySafe(this string? s, string[] formats, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null) =>
            TimeOnly.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

        public static TimeOnly? ToTimeOnlySafe(this ReadOnlySpan<char> s, string[] formats, DateTimeStyles style = Styles.TimeOnly, IFormatProvider? provider = null) =>
            TimeOnly.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;
    }
}
#endif
