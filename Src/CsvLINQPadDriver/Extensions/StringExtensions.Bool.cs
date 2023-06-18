using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static bool? ToBool(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return s.ToLong(style, provider) != 0;
            }
            catch
            {
                // ignored
            }

            try
            {
                return bool.Parse(s);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("bool", s, e);
            }
        }

#if NETCOREAPP
        public static bool? ToBool(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return s.ToLong(style, provider) != 0;
            }
            catch
            {
                // ignored
            }

            try
            {
                return bool.Parse(s);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("bool", s, e);
            }
        }
#endif

        public static bool? ToBoolSafe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            var longValue = s.ToLongSafe(style, provider);

            return longValue.HasValue
                ? longValue.Value != 0
                : bool.TryParse(s, out var parsedValue) ? parsedValue : null;
        }

#if NETCOREAPP
        public static bool? ToBoolSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            var longValue = s.ToLongSafe(style, provider);

            return longValue.HasValue
                ? longValue.Value != 0
                : bool.TryParse(s, out var parsedValue) ? parsedValue : null;
        }
#endif
    }
}
