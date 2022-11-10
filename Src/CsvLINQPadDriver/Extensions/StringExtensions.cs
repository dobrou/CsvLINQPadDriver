using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static class StringExtensions
    {
        public static class Styles
        {
            public const NumberStyles Integer          = NumberStyles.Integer | NumberStyles.AllowThousands;
            public const NumberStyles Float            = NumberStyles.Float   | NumberStyles.AllowThousands;
            public const NumberStyles Decimal          = NumberStyles.Number;

            public const DateTimeStyles DateTime       = DateTimeStyles.None;
            public const DateTimeStyles DateTimeOffset = DateTimeStyles.None;
            public const DateTimeStyles UtcDateTime    = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
            public const TimeSpanStyles TimeSpan       = TimeSpanStyles.None;

#if NET6_0_OR_GREATER
            public const DateTimeStyles DateOnly       = DateTimeStyles.None;
            public const DateTimeStyles TimeOnly       = DateTimeStyles.None;
#endif
        }

        public static readonly CultureInfo DefaultCultureInfo = CultureInfo.InvariantCulture;

        public static sbyte? ToSByte(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(sbyte.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static sbyte? ToSByte(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(sbyte.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static byte? ToByte(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(byte.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static byte? ToByte(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(byte.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static short? ToShort(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(short.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static short? ToShort(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(short.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static ushort? ToUShort(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(ushort.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static ushort? ToUShort(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(ushort.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static int? ToInt(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(int.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static int? ToInt(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(int.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static uint? ToUInt(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(uint.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static uint? ToUInt(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(uint.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static long? ToLong(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(long.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static long? ToLong(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(long.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static ulong? ToULong(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(ulong.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static ulong? ToULong(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(ulong.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

#if NET5_0_OR_GREATER
        public static nint? ToNInt(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(nint.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NET6_0_OR_GREATER
        public static nint? ToNInt(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(nint.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static nuint? ToNUInt(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(nuint.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NET6_0_OR_GREATER
        public static nuint? ToNUInt(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(nuint.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif
#endif

        public static float? ToFloat(this string? str, NumberStyles style = Styles.Float, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(float.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static float? ToFloat(this ReadOnlySpan<char> str, NumberStyles style = Styles.Float, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(float.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static double? ToDouble(this string? str, NumberStyles style = Styles.Float, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(double.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static double? ToDouble(this ReadOnlySpan<char> str, NumberStyles style = Styles.Float, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(double.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static decimal? ToDecimal(this string? str, NumberStyles style = Styles.Decimal, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(decimal.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static decimal? ToDecimal(this ReadOnlySpan<char> str, NumberStyles style = Styles.Decimal, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(decimal.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

#if NET5_0_OR_GREATER
        public static Half? ToHalf(this string? str, NumberStyles style = Styles.Float, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(Half.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static Half? ToHalf(this ReadOnlySpan<char> str, NumberStyles style = Styles.Float, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(Half.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

#if NET7_0_OR_GREATER
        public static Int128? ToInt128(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(Int128.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static Int128? ToInt128(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(Int128.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static UInt128? ToUInt128(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(UInt128.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static UInt128? ToUInt128(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(UInt128.TryParse(str, style, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static DateTime? ToDateTime(this string? str, DateTimeStyles style = Styles.DateTime, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> str, DateTimeStyles style = Styles.DateTime, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static DateTime? ToDateTime(this string? str, string format, DateTimeStyles style = Styles.DateTime, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateTime, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static DateTime? ToDateTime(this string? str, string[] formats, DateTimeStyles style = Styles.DateTime, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> str, string[] formats, DateTimeStyles style = Styles.DateTime, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static DateTime? ToUtcDateTime(this string? str, DateTimeStyles style = Styles.UtcDateTime, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToUtcDateTime(this ReadOnlySpan<char> str, DateTimeStyles style = Styles.UtcDateTime, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static DateTimeOffset? ToDateTimeOffset(this string? str, DateTimeStyles style = Styles.DateTimeOffset, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> str, DateTimeStyles style = Styles.DateTimeOffset, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static DateTimeOffset? ToDateTimeOffset(this string? str, string format, DateTimeStyles style = Styles.DateTimeOffset, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateTimeOffset, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static DateTimeOffset? ToDateTimeOffset(this string? str, string[] formats, DateTimeStyles style = Styles.DateTimeOffset, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> str, string[] formats, DateTimeStyles style = Styles.DateTimeOffset, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static TimeSpan? ToTimeSpan(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParse(str, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParse(str, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static TimeSpan? ToTimeSpan(this string? str, string format, TimeSpanStyles style = Styles.TimeSpan, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, TimeSpanStyles style = Styles.TimeSpan, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static TimeSpan? ToTimeSpan(this string? str, string[] formats, TimeSpanStyles style = Styles.TimeSpan, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> str, string[] formats, TimeSpanStyles style = Styles.TimeSpan, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

#if NET6_0_OR_GREATER
        public static DateOnly? ToDateOnly(this string? str, DateTimeStyles style = Styles.DateOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> str, DateTimeStyles style = Styles.DateOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this string? str, string format, DateTimeStyles style = Styles.DateOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, DateTimeStyles style = Styles.DateOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this string? str, string[] formats, DateTimeStyles style = Styles.DateOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> str, string[] formats, DateTimeStyles style = Styles.DateOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this string? str, DateTimeStyles style = Styles.TimeOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> str, DateTimeStyles style = Styles.TimeOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParse(str, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this string? str, string format, DateTimeStyles style = Styles.TimeOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, DateTimeStyles style = Styles.TimeOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParseExact(str, format, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this string? str, string[] formats, DateTimeStyles style = Styles.TimeOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> str, string[] formats, DateTimeStyles style = Styles.TimeOnly, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParseExact(str, formats, SelectCulture(cultureInfo), style, out var parsedValue), parsedValue);
#endif

        public static Guid? ToGuid(this string? str) =>
            GetValueOrNull(Guid.TryParse(str, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static Guid? ToGuid(this ReadOnlySpan<char> str) =>
            GetValueOrNull(Guid.TryParse(str, out var parsedValue), parsedValue);
#endif

        public static Guid? ToGuid(this string? str, string format) =>
            GetValueOrNull(Guid.TryParseExact(str, format, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static Guid? ToGuid(this ReadOnlySpan<char> str, ReadOnlySpan<char> format) =>
            GetValueOrNull(Guid.TryParseExact(str, format, out var parsedValue), parsedValue);
#endif

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static Guid? ToGuid(this string? str, string[] formats) =>
            formats
                .Select(format => GetValueOrNull(Guid.TryParseExact(str, format, out var parsedValue), parsedValue))
                .FirstOrDefault(static guid => guid is not null);

#if NETCOREAPP
        public static Guid? ToGuid(this ReadOnlySpan<char> str, string[] formats)
        {
            foreach (var format in formats)
            {
                var guid = GetValueOrNull(Guid.TryParseExact(str, format, out var parsedValue), parsedValue);
                if (guid is not null)
                {
                    return guid;
                }
            }

            return null;
        }
#endif

        public static bool? ToBool(this string? str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null)
        {
            var longValue = str.ToLong(style, cultureInfo);

            return longValue.HasValue
                ? longValue.Value != 0
                : GetValueOrNull(bool.TryParse(str, out var parsedValue), parsedValue); 
        }

#if NETCOREAPP
        public static bool? ToBool(this ReadOnlySpan<char> str, NumberStyles style = Styles.Integer, CultureInfo? cultureInfo = null)
        {
            var longValue = str.ToLong(style, cultureInfo);

            return longValue.HasValue
                ? longValue.Value != 0
                : GetValueOrNull(bool.TryParse(str, out var parsedValue), parsedValue); 
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CultureInfo SelectCulture(CultureInfo? cultureInfo) =>
            cultureInfo ?? DefaultCultureInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T? GetValueOrNull<T>(bool converted, T value) where T: struct =>
            converted ? value : null;
    }
}
