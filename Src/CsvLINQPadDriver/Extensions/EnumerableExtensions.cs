using System;
using System.Collections.Generic;

namespace CsvLINQPadDriver.Extensions;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> SkipExceptions<T>(this IEnumerable<T> source, ICollection<Exception>? exceptions = null)
    {
        using var enumerator = source.GetEnumerator();

        while (true)
        {
            try
            {
                if (!enumerator.MoveNext())
                {
                    break;
                }
            }
            catch (Exception exception) when (exception.CanBeHandled())
            {
                exceptions?.Add(exception);
                continue;
            }

            yield return enumerator.Current;
        }
    }
}
