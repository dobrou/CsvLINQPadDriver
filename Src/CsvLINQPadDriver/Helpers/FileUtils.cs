using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Humanizer;

using CsvHelper;
using CsvHelper.Configuration;

using CsvLINQPadDriver.CodeGen;

namespace CsvLINQPadDriver.Helpers
{
    public static class FileUtils
    {
        private static readonly Dictionary<string, string> StringInternCache = new Dictionary<string, string>();

        private static string StringIntern(string str) =>
            str switch
            {
                null => null,
                _ => StringInternCache.TryGetValue(str, out var intern)
                        ? intern
                        : StringInternCache[str] = str
            };

        public static IEnumerable<T> CsvReadRows<T>(string fileName, char csvSeparator, bool internString, CsvRowMappingBase<T> csvClassMap)
            where T : ICsvRowBase, new()
        {
            return CsvReadRows(fileName, csvSeparator)
                .Skip(1) // Skip header.
                .Select(GetRecord);

            T GetRecord(string[] rowColumns)
            {
                if (internString)
                {
                    for (var i = 0; i < rowColumns.Length; i++)
                    {
                        rowColumns[i] = StringIntern(rowColumns[i]);
                    }
                }

                return csvClassMap.InitRowObject(rowColumns);
            }
        }

        private static IEnumerable<string[]> CsvReadRows(string fileName, char csvSeparator)
        {
            using var csvParser = CreateCsvParser(fileName, csvSeparator);

            while (csvParser.Read())
            {
                yield return csvParser.Record;
            }
        }

        public static string[] CsvReadHeader(string fileName, char csvSeparator)
        {
            using var csvParser = CreateCsvParser(fileName, csvSeparator);

            return csvParser.Read() ? csvParser.Record : new string[0];
        }

        public static bool IsCsvFormatValid(string fileName, char csvSeparator)
        {
            var header = $"{fileName} is not valid CSV file:";

            if (!File.Exists(fileName))
            {
                CsvDataContextDriver.WriteToLog($"{header} file does not exist");

                return false;
            }

            try
            {
                using var csvParser = CreateCsvParser(fileName, csvSeparator);

                if (!csvParser.Read())
                {
                    CsvDataContextDriver.WriteToLog($"{header} could not get CSV header");

                    return false;
                }

                var headerRow = csvParser.Record;

                // 0 or 1 column.
                if (headerRow.Length <= 1)
                {
                    CsvDataContextDriver.WriteToLog($"{header} CSV header had no columns");

                    return false;
                }

                if (!csvParser.Read())
                {
                    CsvDataContextDriver.WriteToLog($"{header} CSV has header but has no data");

                    return false;
                }

                var dataRow = csvParser.Record;

                // Column count differs.
                if (headerRow.Length != dataRow.Length)
                {
                    CsvDataContextDriver.WriteToLog($"{header} CSV header column count does not match data column count");

                    return false;
                }

                // Too many strange characters.
                var charsCount = headerRow
                    .Concat(dataRow)
                    .Sum(s => s?.Length ?? 0);

                var validCharsCount = headerRow
                    .Concat(dataRow)
                    .Sum(s => Enumerable.Range(0, s?.Length ?? 0).Count(i => char.IsLetterOrDigit(s ?? string.Empty, i)));

                const double validCharsMinOkRatio = 0.5;

                return validCharsCount >= validCharsMinOkRatio * charsCount;
            }
            catch(Exception exception)
            {
                CsvDataContextDriver.WriteToLog($"{header} failed with exception", exception);

                return false;
            }
        }

        public static char CsvDetectSeparator(string fileName, string[] csvData = null)
        {
            var defaultCsvSeparators = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                "tsv" => new[] { '\t', ',', ';' },
                _ => new[] { ',', ';', '\t' }
            };

            var result = defaultCsvSeparators.First();

            if (!File.Exists(fileName))
            {
                return result;
            }

            try
            {
                // Get most used char from separators as separator.
                var bestSeparators = (csvData ?? File.ReadLines(fileName).Take(1).ToArray())
                    .SelectMany(l => l.ToCharArray())
                    .Where(defaultCsvSeparators.Contains)
                    .GroupBy(ch => ch)
                    .OrderByDescending(chGroup => chGroup.Count())
                    .Select(chGroup => chGroup.Key)
                    .ToArray();

                if (bestSeparators.Any())
                {
                    result = bestSeparators.First();
                }
            }
            catch(Exception exception)
            {
                CsvDataContextDriver.WriteToLog($"CSV separator detection failed for {fileName}", exception);
            }

            CsvDataContextDriver.WriteToLog($"Using CSV separator '{result}' for {fileName}");

            return result;
        }

        public static string GetLongestCommonPrefixPath(string[] paths)
        {
            var pathsValid = GetFiles(paths).ToArray();

            // Get longest common path prefix.
            var filePaths = pathsValid.FirstOrDefault()?.Split(Path.DirectorySeparatorChar) ?? new string[0];

            return Enumerable.Range(1, filePaths.Length)
                .Select(i => string.Join(Path.DirectorySeparatorChar, filePaths.Take(i).ToArray()))
                .LastOrDefault(prefix => pathsValid.All(path => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))) ?? string.Empty;
        }

        public static string[] EnumFiles(IEnumerable<string> paths) =>
            GetFiles(paths)
                .SelectMany(EnumFiles)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        public static string GetHumanizedFileSize(string fileName)
        {
            try
            {
                return new FileInfo(fileName).Length.Bytes().Humanize("0.#");
            }
            catch (Exception exception)
            {
                CsvDataContextDriver.WriteToLog($"Failed to get {fileName} size", exception);

                return exception.Message;
            }
        }

        private static CsvParser CreateCsvParser(string fileName, char csvSeparator)
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = csvSeparator.ToString(),
                HasHeaderRecord = false,
                DetectColumnCountChanges = false,
                BufferSize = 4096 * 20
            };

            return new CsvParser(new StreamReader(fileName, Encoding.Default, true, csvConfiguration.BufferSize / sizeof(char)), csvConfiguration);
        }

        private static IEnumerable<string> GetFiles(IEnumerable<string> paths) =>
            paths
                .Where(p => !p.StartsWith("#"))
                .Where(p => !string.IsNullOrWhiteSpace(p));

        private static string[] EnumFiles(string path)
        {
            try
            {
                path ??= string.Empty;

                // Single file.
                if (File.Exists(path))
                {
                    return new[] { path };
                }

                var file = Path.GetFileName(path);

                string baseDir;

                var isPathOnlyDir = string.IsNullOrEmpty(file) || Directory.Exists(path);
                if (isPathOnlyDir)
                {
                    file = "*.csv";
                    baseDir = path;
                }
                else
                {
                    baseDir = Path.GetDirectoryName(path) ?? string.Empty;
                }

                if (!Directory.Exists(baseDir))
                {
                    return new string[0];
                }

                return Directory
                    .EnumerateFiles(baseDir, file, file.Contains("**") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .ToArray();
            }
            catch(Exception exception)
            {
                CsvDataContextDriver.WriteToLog($"File enumeration failed for {path}", exception);

                return new string[0];
            }
        }
    }
}
