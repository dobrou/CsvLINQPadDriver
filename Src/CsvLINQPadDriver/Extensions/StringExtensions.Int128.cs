#if NET7_0_OR_GREATER
using System;
using System.Globalization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static Int128? ToInt128(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return Int128.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("Int128", s, e);
            }
        }

        public static Int128? ToInt128(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return Int128.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("Int128", s, e);
            }
        }

        public static UInt128? ToUInt128(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return UInt128.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("UInt128", s, e);
            }
        }

        public static UInt128? ToUInt128(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return UInt128.Parse(s, style, provider.ResolveFormatProvider());
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("UInt128", s, e);
            }
        }

        public static Int128? ToInt128Safe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            Int128.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

        public static Int128? ToInt128Safe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            Int128.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

        public static UInt128? ToUInt128Safe(this string? s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            UInt128.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;

        public static UInt128? ToUInt128Safe(this ReadOnlySpan<char> s, NumberStyles style = Styles.Integer, IFormatProvider? provider = null) =>
            UInt128.TryParse(s, style, provider.ResolveFormatProvider(), out var parsedValue) ? parsedValue : null;
    }
}
#endif
