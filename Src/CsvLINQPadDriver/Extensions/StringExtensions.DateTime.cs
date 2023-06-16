using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static DateTime? ToDateTime(this string? s, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateTime.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateTime.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }
#endif

        public static DateTime? ToDateTime(this string? s, string format, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateTime.ParseExact(s, format, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateTime.ParseExact(s, format, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }
#endif

        public static DateTime? ToDateTime(this string? s, string[] formats, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateTime.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> s, string[] formats, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateTime.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }
#endif

        public static DateTime? ToUtcDateTime(this string? s, DateTimeStyles style = Styles.UtcDateTime, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateTime.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }

#if NETCOREAPP
        public static DateTime? ToUtcDateTime(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.UtcDateTime, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateTime.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }
#endif

#if NETCOREAPP
        public static DateTime? ToUtcDateTimeFromUnixTimeSeconds(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return s.ToDateTimeOffsetFromUnixTimeSeconds(style, provider).ToUtcDateTimeFromDateTimeOffset();
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }
#endif

        public static DateTime? ToUtcDateTimeFromUnixTimeSeconds(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return s.ToDateTimeOffsetFromUnixTimeSeconds(style, provider).ToUtcDateTimeFromDateTimeOffset();
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }

#if NETCOREAPP
        public static DateTime? ToUtcDateTimeFromUnixTimeMilliseconds(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return s.ToDateTimeOffsetFromUnixTimeMilliseconds(style, provider).ToUtcDateTimeFromDateTimeOffset();
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }
#endif

        public static DateTime? ToUtcDateTimeFromUnixTimeMilliseconds(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return s.ToDateTimeOffsetFromUnixTimeMilliseconds(style, provider).ToUtcDateTimeFromDateTimeOffset();
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTime", s, e);
            }
        }

        public static DateTime? ToDateTimeSafe(this string? s, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTime.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTimeSafe(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTime.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);
#endif

        public static DateTime? ToDateTimeSafe(this string? s, string format, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTime.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTimeSafe(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTime.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);
#endif

        public static DateTime? ToDateTimeSafe(this string? s, string[] formats, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTime.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTimeSafe(this ReadOnlySpan<char> s, string[] formats, DateTimeStyles style = Styles.DateTime, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTime.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);
#endif

        public static DateTime? ToUtcDateTimeSafe(this string? s, DateTimeStyles style = Styles.UtcDateTime, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTime.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToUtcDateTimeSafe(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.UtcDateTime, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTime.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);
#endif

        private static DateTime? ToUtcDateTimeFromDateTimeOffset(this DateTimeOffset? dateTimeOffset) =>
            dateTimeOffset is null ? null : new DateTime(dateTimeOffset.Value.Ticks, DateTimeKind.Utc);

        private static DateTime? ToUtcDateTimeFromDateTimeOffsetSafe(this DateTimeOffset? dateTimeOffset)
        {
            try
            {
                return dateTimeOffset is null ? null : new DateTime(dateTimeOffset.Value.Ticks, DateTimeKind.Utc);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                return null;
            }
        }

#if NETCOREAPP
        public static DateTime? ToUtcDateTimeFromUnixTimeSecondsSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            s.ToDateTimeOffsetFromUnixTimeSecondsSafe(style, provider).ToUtcDateTimeFromDateTimeOffsetSafe();
#endif

        public static DateTime? ToUtcDateTimeFromUnixTimeSecondsSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            s.ToDateTimeOffsetFromUnixTimeSecondsSafe(style, provider).ToUtcDateTimeFromDateTimeOffsetSafe();

#if NETCOREAPP
        public static DateTime? ToUtcDateTimeFromUnixTimeMillisecondsSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            s.ToDateTimeOffsetFromUnixTimeMillisecondsSafe(style, provider).ToUtcDateTimeFromDateTimeOffsetSafe();
#endif

        public static DateTime? ToUtcDateTimeFromUnixTimeMillisecondsSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            s.ToDateTimeOffsetFromUnixTimeMillisecondsSafe(style, provider).ToUtcDateTimeFromDateTimeOffsetSafe();
    }
}
