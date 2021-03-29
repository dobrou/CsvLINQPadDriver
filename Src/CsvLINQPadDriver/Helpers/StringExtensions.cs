using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CsvLINQPadDriver.Helpers
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class StringExtensions
    {
        private static readonly CultureInfo DefaultCultureInfo = CultureInfo.InvariantCulture;

        public static int? ToInt(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(int.TryParse(str, NumberStyles.Integer, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static long? ToLong(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(long.TryParse(str, NumberStyles.Integer, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static double? ToDouble(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static decimal? ToDecimal(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(decimal.TryParse(str, NumberStyles.Number, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static DateTime? ToDateTime(this string? str, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParse(str, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static DateTime? ToDateTime(this string? str, string format, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, format, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static DateTime? ToDateTime(this string? str, string[] formats, DateTimeStyles dateTimeStyles = DateTimeStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(DateTime.TryParseExact(str, formats, SelectCulture(cultureInfo), dateTimeStyles, out var parsedValue), parsedValue);

        public static TimeSpan? ToTimeSpan(this string? str, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParse(str, SelectCulture(cultureInfo), out var parsedValue), parsedValue);

        public static TimeSpan? ToTimeSpan(this string? str, string format, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, format, SelectCulture(cultureInfo), timeSpanStyles, out var parsedValue), parsedValue);

        public static TimeSpan? ToTimeSpan(this string? str, string[] formats, TimeSpanStyles timeSpanStyles = TimeSpanStyles.None, CultureInfo? cultureInfo = null) =>
            GetValueOrNull(TimeSpan.TryParseExact(str, formats, SelectCulture(cultureInfo), timeSpanStyles, out var parsedValue), parsedValue);

        public static Guid? ToGuid(this string? str) =>
            GetValueOrNull(Guid.TryParse(str, out var parsedValue), parsedValue);

        public static Guid? ToGuid(this string? str, string format) =>
            GetValueOrNull(Guid.TryParseExact(str, format, out var parsedValue), parsedValue);

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static Guid? ToGuid(this string? str, string[] formats) =>
            formats
                .Select(format => GetValueOrNull(Guid.TryParseExact(str, format, out var parsedValue), parsedValue))
                .FirstOrDefault(guid => guid is not null);

        public static bool? ToBool(this string? str, CultureInfo? cultureInfo = null)
        {
            var longValue = str.ToLong(cultureInfo);

            return longValue.HasValue
                ? longValue.Value != 0
                : GetValueOrNull(bool.TryParse(str, out var parsedValue), parsedValue); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CultureInfo SelectCulture(CultureInfo? cultureInfo) =>
            cultureInfo ?? DefaultCultureInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T? GetValueOrNull<T>(bool converted, T value) where T: struct =>
            converted ? value : (T?)null;
    }
}
