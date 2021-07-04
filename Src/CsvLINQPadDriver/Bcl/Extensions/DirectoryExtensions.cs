using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CsvLINQPadDriver.Extensions;

namespace CsvLINQPadDriver.Bcl.Extensions
{
    internal static class DirectoryExtensions
    {
        public static IEnumerable<string> EnumerateFiles(this ICollection<Exception>? exceptions, string path, string searchPattern, SearchOption searchOption)
        {
            var directories = new Queue<string>(new[] { path });

            while (directories.Any())
            {
                var currentDirectory = directories.Dequeue();

                if (searchOption == SearchOption.AllDirectories)
                {
                    try
                    {
                        foreach (var directory in Directory.EnumerateDirectories(currentDirectory))
                        {
                            directories.Enqueue(directory);
                        }
                    }
                    catch (Exception e)
                    {
                        exceptions?.Add(e);
                        continue;
                    }
                }

                foreach (var file in Directory.EnumerateFiles(currentDirectory, searchPattern).SkipExceptions(exceptions))
                {
                    yield return file;
                }
            }
        }
    }
}
