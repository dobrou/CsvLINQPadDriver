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
            int v;
            return int.TryParse(s, out v) ? v : (int?)null;
        }

        public static long? ToLong(this string s) {
            long v;
            return long.TryParse(s, out v) ? v : (long?)null;
        }

        public static double? ToDouble(this string s) 
        {
            double v;
            return double.TryParse(s, out v) ? v : (double?)null;
        }

        public static DateTime? ToDateTime(this string s) 
        {
            DateTime v;
            return DateTime.TryParse(s, out v) ? v : (DateTime?) null;
        }

        public static bool? ToBool(this string s)
        {
            if (s != null)
            {
                if (s == "1")
                    return true;
                if (s == "0")
                    return false;
            }

            bool v;
            return bool.TryParse(s, out v) ? v : (bool?) null;
        }
    }
}
