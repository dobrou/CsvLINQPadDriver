using System;

namespace CsvLINQPadDriver.Helpers
{

    /// <summary>
    /// Provides easy conversion from string to basic types.
    /// </summary>
    public static class StringExtensions
    {
        public static int? ToInt(this string s) 
        {
            return int.TryParse(s, out var v) ? v : (int?)null;
        }

        public static long? ToLong(this string s)
        {
            return long.TryParse(s, out var v) ? v : (long?)null;
        }

        public static double? ToDouble(this string s) 
        {
            return double.TryParse(s, out var v) ? v : (double?)null;
        }

        public static decimal? ToDecimal(this string s)
        {
            return decimal.TryParse(s, out var v) ? v : (decimal?)null;
        }

        public static DateTime? ToDateTime(this string s)
        {
            return DateTime.TryParse(s, out var v) ? v : (DateTime?) null;
        }

        public static TimeSpan? ToTimeSpan(this string s)
        {
            return TimeSpan.TryParse(s, out var v) ? v : (TimeSpan?)null;
        }

        public static bool? ToBool(this string s)
        {
            var toLong = s.ToLong();
            if (toLong.HasValue)
                return toLong.Value != 0;

            return bool.TryParse(s, out var v) ? v : (bool?) null;
        }
    }
}
