using System;
using System.Globalization;
using System.Runtime.CompilerServices;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.Extensions
{
    public static partial class StringExtensions
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

        public static readonly IFormatProvider DefaultFormatProvider = CultureInfo.InvariantCulture;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IFormatProvider ResolveFormatProvider(this IFormatProvider? provider) =>
            provider ?? DefaultFormatProvider;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T? GetValueOrNull<T>(bool converted, T value) where T : struct =>
            converted ? value : null;
    }
}
