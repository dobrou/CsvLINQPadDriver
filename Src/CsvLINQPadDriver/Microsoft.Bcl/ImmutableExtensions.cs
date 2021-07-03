using System.Collections.Generic;
using System.Linq;

namespace CsvLINQPadDriver.Microsoft.Bcl
{
    internal static class ImmutableExtensions
    {
        public static List<T> ToImmutableList<T>(this IEnumerable<T> source) =>
            source.ToList();
    }
}
