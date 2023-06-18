using System;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
    {
        public static Guid? ToGuid(this string? s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return Guid.Parse(s);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("Guid", s, e);
            }
        }

#if NETCOREAPP
        public static Guid? ToGuid(this ReadOnlySpan<char> s)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return Guid.Parse(s);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("Guid", s, e);
            }
        }
#endif

        public static Guid? ToGuid(this string? s, string format)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            try
            {
                return Guid.ParseExact(s, format);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("Guid", s, e);
            }
        }

#if NETCOREAPP
        public static Guid? ToGuid(this ReadOnlySpan<char> s, ReadOnlySpan<char> format)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            try
            {
                return Guid.ParseExact(s, format);
            }
            catch (Exception e) when (e.CanBeHandled())
            {
                throw ConvertException.Create("Guid", s, e);
            }
        }
#endif

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static Guid? ToGuid(this string? s, string[] formats)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            foreach (var format in formats)
            {
                if (Guid.TryParseExact(s, format, out var guid))
                {
                    return guid;
                }
            }

            throw ConvertException.Create("Guid", s, null);
        }

#if NETCOREAPP
        public static Guid? ToGuid(this ReadOnlySpan<char> s, string[] formats)
        {
            if (s.IsEmpty)
            {
                return null;
            }

            foreach (var format in formats)
            {
                if (Guid.TryParseExact(s, format, out var guid))
                {
                    return guid;
                }
            }

            throw ConvertException.Create("Guid", s, null);
        }
#endif

        public static Guid? ToGuidSafe(this string? s) =>
            Guid.TryParse(s, out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static Guid? ToGuidSafe(this ReadOnlySpan<char> s) =>
            Guid.TryParse(s, out var parsedValue) ? parsedValue : null;
#endif

        public static Guid? ToGuidSafe(this string? s, string format) =>
            Guid.TryParseExact(s, format, out var parsedValue) ? parsedValue : null;

#if NETCOREAPP
        public static Guid? ToGuidSafe(this ReadOnlySpan<char> s, ReadOnlySpan<char> format) =>
            Guid.TryParseExact(s, format, out var parsedValue) ? parsedValue : null;
#endif

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static Guid? ToGuidSafe(this string? s, string[] formats) =>
            formats
                .Select(format => Guid.TryParseExact(s, format, out var parsedValue) ? parsedValue : (Guid?)null)
                .FirstOrDefault(static guid => guid is not null);

#if NETCOREAPP
        public static Guid? ToGuidSafe(this ReadOnlySpan<char> s, string[] formats)
        {
            foreach (var format in formats)
            {
                if (Guid.TryParseExact(s, format, out var parsedValue))
                {
                    return parsedValue;
                }
            }

            return null;
        }
#endif
    }
}
