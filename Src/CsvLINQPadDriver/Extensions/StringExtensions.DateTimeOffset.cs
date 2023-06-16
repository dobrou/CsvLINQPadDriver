using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static DateTimeOffset? ToDateTimeOffset(this string? s, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateTimeOffset.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateTimeOffset.Parse(s, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }
#endif

        public static DateTimeOffset? ToDateTimeOffset(this string? s, string format, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateTimeOffset.ParseExact(s, format, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateTimeOffset.ParseExact(s, format, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }
#endif

        public static DateTimeOffset? ToDateTimeOffset(this string? s, string[] formats, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return DateTimeOffset.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> s, string[] formats, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return DateTimeOffset.ParseExact(s, formats, provider.ResolveFormatProvider(), style);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }
#endif

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffsetFromUnixTimeSeconds(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                var longVar = s.ToLong(style, provider);
                return longVar is null ? null : DateTimeOffset.FromUnixTimeSeconds(longVar.Value);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }
#endif

        public static DateTimeOffset? ToDateTimeOffsetFromUnixTimeSeconds(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                var longVar = s.ToLong(style, provider);
                return longVar is null ? null : DateTimeOffset.FromUnixTimeSeconds(longVar.Value);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffsetFromUnixTimeMilliseconds(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                var longVar = s.ToLong(style, provider);
                return longVar is null ? null : DateTimeOffset.FromUnixTimeMilliseconds(longVar.Value);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }
#endif

        public static DateTimeOffset? ToDateTimeOffsetFromUnixTimeMilliseconds(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                var longVar = s.ToLong(style, provider);
                return longVar is null ? null : DateTimeOffset.FromUnixTimeMilliseconds(longVar.Value);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("DateTimeOffset", s, e);
            }
        }

        public static DateTimeOffset? ToDateTimeOffsetSafe(this string? s, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTimeOffset.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffsetSafe(this ReadOnlySpan<char> s, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTimeOffset.TryParse(s, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);
#endif

        public static DateTimeOffset? ToDateTimeOffsetSafe(this string? s, string format, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffsetSafe(this ReadOnlySpan<char> s, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(s, format, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);
#endif

        public static DateTimeOffset? ToDateTimeOffsetSafe(this string? s, string[] formats, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffsetSafe(this ReadOnlySpan<char> s, string[] formats, DateTimeStyles style = Styles.DateTimeOffset, IFormatProvider? provider = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(s, formats, provider.ResolveFormatProvider(), style, out var parsedValue), parsedValue);
#endif

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffsetFromUnixTimeSecondsSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                var longVar = s.ToLongSafe(style, provider);
                return longVar is null ? null : DateTimeOffset.FromUnixTimeSeconds(longVar.Value);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                return null;
            }
        }
#endif

        public static DateTimeOffset? ToDateTimeOffsetFromUnixTimeSecondsSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                var longVar = s.ToLongSafe(style, provider);
                return longVar is null ? null : DateTimeOffset.FromUnixTimeSeconds(longVar.Value);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                return null;
            }
        }

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffsetFromUnixTimeMillisecondsSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                var longVar = s.ToLongSafe(style, provider);
                return longVar is null ? null : DateTimeOffset.FromUnixTimeMilliseconds(longVar.Value);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                return null;
            }
        }
#endif

        public static DateTimeOffset? ToDateTimeOffsetFromUnixTimeMillisecondsSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                var longVar = s.ToLongSafe(style, provider);
                return longVar is null ? null : DateTimeOffset.FromUnixTimeMilliseconds(longVar.Value);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                return null;
            }
        }
    }
}
