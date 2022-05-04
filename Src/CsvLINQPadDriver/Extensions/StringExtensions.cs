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
        private static readonly CultureInfo DefaultCultureInfo = CultureInfo.InvariantCulture;

        public static int? ToInt(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(int.TryParse(str, NumberStyles.Integer, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static int? ToInt(this ReadOnlySpan<char> str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(int.TryParse(str, NumberStyles.Integer, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static long? ToLong(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(long.TryParse(str, NumberStyles.Integer, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static long? ToLong(this ReadOnlySpan<char> str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(long.TryParse(str, NumberStyles.Integer, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static float? ToFloat(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(float.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static float? ToFloat(this ReadOnlySpan<char> str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(float.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static double? ToDouble(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static double? ToDouble(this ReadOnlySpan<char> str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static decimal? ToDecimal(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(decimal.TryParse(str, NumberStyles.Number, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static decimal? ToDecimal(this ReadOnlySpan<char> str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(decimal.TryParse(str, NumberStyles.Number, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static DateTime? ToDateTime(this string? str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);
#endif

        public static DateTime? ToDateTime(this string? str, string format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);
#endif

        public static DateTime? ToDateTime(this string? str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTime? ToDateTime(this ReadOnlySpan<char> str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);
#endif

        public static DateTimeOffset? ToDateTimeOffset(this string? str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);
#endif

        public static DateTimeOffset? ToDateTimeOffset(this string? str, string format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);
#endif

        public static DateTimeOffset? ToDateTimeOffset(this string? str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static DateTimeOffset? ToDateTimeOffset(this ReadOnlySpan<char> str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTimeOffset.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);
#endif

        public static TimeSpan? ToTimeSpan(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParse(str, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParse(str, SelectCulture(cultureInfo), out var parsedValue), parsedValue);
#endif

        public static TimeSpan? ToTimeSpan(this string? str, string format, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, format, SelectCulture(cultureInfo), timeSpanStyles, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, format, SelectCulture(cultureInfo), timeSpanStyles, out var parsedValue), parsedValue);
#endif

        public static TimeSpan? ToTimeSpan(this string? str, string[] formats, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, formats, SelectCulture(cultureInfo), timeSpanStyles, out var parsedValue), parsedValue);

#if NETCOREAPP
        public static TimeSpan? ToTimeSpan(this ReadOnlySpan<char> str, string[] formats, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, formats, SelectCulture(cultureInfo), timeSpanStyles, out var parsedValue), parsedValue);
#endif

#if NET6_0_OR_GREATER
        public static DateOnly? ToDateOnly(this string? str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this string? str, string format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this string? str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static DateOnly? ToDateOnly(this ReadOnlySpan<char> str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateOnly.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this string? str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this string? str, string format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> str, ReadOnlySpan<char> format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this string? str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static TimeOnly? ToTimeOnly(this ReadOnlySpan<char> str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeOnly.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);
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

        public static bool? ToBool(this string? str, CultureInfo? cultureInfo = null)
        {
            var longValue = str.ToLong(cultureInfo);

            return longValue.HasValue
                ? longValue.Value != 0
                : GetValueOrNull(bool.TryParse(str, out var parsedValue), parsedValue); 
        }

#if NETCOREAPP
        public static bool? ToBool(this ReadOnlySpan<char> str, CultureInfo? cultureInfo = null)
        {
            var longValue = str.ToLong(cultureInfo);

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
