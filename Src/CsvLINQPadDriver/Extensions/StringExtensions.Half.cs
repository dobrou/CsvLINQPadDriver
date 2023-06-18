#if NET5_0_OR_GREATER
using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static Half? ToHalf(this string? s, NumberStyles style = Styles.Float, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return Half.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("Half", s, e);
            }
        }

        public static Half? ToHalf(this ReadOnlySpan<char> s, NumberStyles style = Styles.Float, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return Half.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("Half", s, e);
            }
        }

        public static Half? ToHalfSafe(this string? s, NumberStyles style = Styles.Float, IFormatProvider? provider = null) =>
            Half.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

        public static Half? ToHalfSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Float, IFormatProvider? provider = null) =>
            Half.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
    }
}
#endif
