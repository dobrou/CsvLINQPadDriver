using System;

namespace CsvLINQPadDriver
{
    internal static class Config
    {
        internal static class Regex
        {
#if NET7_0_OR_GREATER
            public
#else
            private
#endif
            const int TimeoutMs = 250;

            public static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(TimeoutMs);
        }
    }
}
