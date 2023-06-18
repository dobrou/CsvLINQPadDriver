using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static float? ToFloat(this string? s, NumberStyles style = Styles.Float, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return float.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("float", s, e);
            }
        }

#if NETCOREAPP
        public static float? ToFloat(this ReadOnlySpan<char> s, NumberStyles style = Styles.Float, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return float.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("float", s, e);
            }
        }
#endif

        public static float? ToFloatSafe(this string? s, NumberStyles style = Styles.Float, IFormatProvider? provider = null) =>
            float.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static float? ToFloatSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Float, IFormatProvider? provider = null) =>
            float.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
#endif
    }
}
