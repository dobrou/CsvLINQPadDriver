using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static long? ToLong(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return long.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("long", s, e);
            }
        }

#if NETCOREAPP
        public static long? ToLong(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return long.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("long", s, e);
            }
        }
#endif

        public static ulong? ToULong(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return ulong.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("ulong", s, e);
            }
        }

#if NETCOREAPP
        public static ulong? ToULong(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return ulong.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("ulong", s, e);
            }
        }
#endif

        public static long? ToLongSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            long.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static long? ToLongSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            long.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif

        public static ulong? ToULongSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            ulong.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static ulong? ToULongSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            ulong.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif
    }
}
