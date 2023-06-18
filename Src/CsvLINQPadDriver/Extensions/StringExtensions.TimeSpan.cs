using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static TimeSpan? ToTimeSpan(this string? s, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return TimeSpan.Parse(s, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeSpan", s, e);
            }
        }

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> s, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return TimeSpan.Parse(s, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeSpan", s, e);
            }
        }
#endif

        public static TimeSpan? ToTimeSpan(this string? s, string format, TimeSpanStyles style = Styles.TimeSpan, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return TimeSpan.ParseExact(s, format, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeSpan", s, e);
            }
        }

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, TimeSpanStyles style = Styles.TimeSpan, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return TimeSpan.ParseExact(s, format, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeSpan", s, e);
            }
        }
#endif

        public static TimeSpan? ToTimeSpan(this string? s, string[] formats, TimeSpanStyles style = Styles.TimeSpan, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return TimeSpan.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeSpan", s, e);
            }
        }

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> s, string[] formats, TimeSpanStyles style = Styles.TimeSpan, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return TimeSpan.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("TimeSpan", s, e);
            }
        }
#endif

        public static TimeSpan? ToTimeSpanSafe(this string? s, IFormatProvider? provider = null) =>
            TimeSpan.TryParse(s, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static TimeSpan? ToTimeSpanSafe(this ReadOnlySpan<char> s, IFormatProvider? provider = null) =>
            TimeSpan.TryParse(s, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif

        public static TimeSpan? ToTimeSpanSafe(this string? s, string format, TimeSpanStyles style = Styles.TimeSpan, IFormatProvider? provider = null) =>
            TimeSpan.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static TimeSpan? ToTimeSpanSafe(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, TimeSpanStyles style = Styles.TimeSpan, IFormatProvider? provider = null) =>
            TimeSpan.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;
#endif

        public static TimeSpan? ToTimeSpanSafe(this string? s, string[] formats, TimeSpanStyles style = Styles.TimeSpan, IFormatProvider? provider = null) =>
            TimeSpan.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static TimeSpan? ToTimeSpanSafe(this ReadOnlySpan<char> s, string[] formats, TimeSpanStyles style = Styles.TimeSpan, IFormatProvider? provider = null) =>
            TimeSpan.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue) ? parsedValue : null;
#endif
    }
}
