using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static decimal? ToDecimal(this string? s, NumberStyles style = Styles.Decimal, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return decimal.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("decimal", s, e);
            }
        }

#if NETCOREAPP
        public static decimal? ToDecimal(this ReadOnlySpan<char> s, NumberStyles style = Styles.Decimal, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return decimal.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("decimal", s, e);
            }
        }
#endif

        public static decimal? ToDecimalSafe(this string? s, NumberStyles style = Styles.Decimal, IFormatProvider? provider = null) =>
            GetValueOrNull(decimal.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static decimal? ToDecimalSafe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Decimal, IFormatProvider? provider = null) =>
            GetValueOrNull(decimal.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue), parsedValue);
#endif
    }
}
