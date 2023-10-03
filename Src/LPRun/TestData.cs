using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace LPRun;

/// <summary>
/// Provides methods for manipulating test data.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Augments the existing test data with file index which can be appended to the file name to create the unique file name.
    /// </summary>
    /// <typeparam name="T">The test data type.</typeparam>
    /// <param name="testData">The test data.</param>
    /// <param name="getFileName">The function which returns the file name from the test data.</param>
    /// <param name="augmentTestData">The function which returns test data augmented with the file name index. The same object or copy can be returned.</param>
    /// <returns>The test data augmented with the file name index.</returns>
    public static IEnumerable<T> AugmentWithFileIndex<T>(this IEnumerable<T> testData, Func<T, string> getFileName, Func<T, int, T> augmentTestData)
    {
        var names = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        return testData.Select(Augment);

        T Augment(T data)
        {
            var fileName = getFileName(data);
            names[fileName] = names.TryGetValue(fileName, out var index) ? ++index : 0;

            return augmentTestData(data, index);
        }
    }
}
