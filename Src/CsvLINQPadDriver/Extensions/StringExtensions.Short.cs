using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static short? ToShort(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return short.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("short", s, e);
            }
        }

#if NETCOREAPP
        public static short? ToShort(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return short.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("short", s, e);
            }
        }
#endif

        public static ushort? ToUShort(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return ushort.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("ushort", s, e);
            }
        }

#if NETCOREAPP
        public static ushort? ToUShort(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return ushort.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("ushort", s, e);
            }
        }
#endif

        public static short? ToShortSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            short.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static short? ToShortSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            short.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif

        public static ushort? ToUShortSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            ushort.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static ushort? ToUShortSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            ushort.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif
    }
}
