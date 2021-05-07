using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Humanizer;

using CsvHelper;
using CsvHelper.Configuration;

using CsvLINQPadDriver.CodeGen;

namespace CsvLINQPadDriver.Helpers
{
    public static class FileUtils
    {
        private const StringComparison FileNameComparison = StringComparison.OrdinalIgnoreCase;

        private static readonly StringComparer FileNameComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly Dictionary<string, string> StringInternCache = new();

        private static readonly Lazy<IReadOnlyDictionary<NoBomEncoding, Encoding>> NoBomEncodings = new(CalculateNoBomEncodings);

        public static IEnumerable<T> CsvReadRows<T>(this string fileName, char? csvSeparator, bool internString, NoBomEncoding noBomEncoding, bool allowComments, CsvRowMappingBase<T> csvClassMap)
            where T : ICsvRowBase, new()
        {
            return CsvReadRows(fileName, csvSeparator, noBomEncoding, allowComments)
                .Skip(1) // Skip header.
                .Select(GetRecord);

            T GetRecord(string?[] rowColumns)
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

        public static string GetDefaultDrive() =>
            Path.GetPathRoot(Environment.SystemDirectory)!.ToLower();

        public static IEnumerable<string> CsvReadHeader(this string fileName, char? csvSeparator, NoBomEncoding noBomEncoding, bool allowComments)
        {
            using var csvParser = CreateCsvParser(fileName, csvSeparator, noBomEncoding, allowComments);

            return csvParser.Read() ? csvParser.Record : new string[0];
        }

        public static bool IsCsvFormatValid(this string fileName, char? csvSeparator, NoBomEncoding noBomEncoding, bool allowComments)
        {
            var header = $"{fileName} is not valid CSV file:";

            if (!File.Exists(fileName))
            {
                CsvDataContextDriver.WriteToLog($"{header} file does not exist");

                return false;
            }

            try
            {
                using var csvParser = CreateCsvParser(fileName, csvSeparator, noBomEncoding, allowComments);

                if (!csvParser.Read())
                {
                    CsvDataContextDriver.WriteToLog($"{header} could not get CSV header");

                    return false;
                }

                var headerRow = csvParser.Record;

                // No columns.
                if (!headerRow.Any())
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

        public static string GetLongestCommonPrefixPath(this IEnumerable<string> paths)
        {
            var pathsValid = paths.GetFilesOnly().ToImmutableList();

            // Get longest common path prefix.
            var filePaths = pathsValid.FirstOrDefault()?.Split(Path.DirectorySeparatorChar) ?? new string[0];

            return Enumerable.Range(1, filePaths.Length)
                .Select(i => string.Join(Path.DirectorySeparatorChar, filePaths.Take(i).ToImmutableList()))
                .LastOrDefault(prefix => pathsValid.All(path => path.StartsWith(prefix, FileNameComparison))) ?? string.Empty;
        }

        public static IEnumerable<string> EnumFiles(this IEnumerable<string> paths) =>
            GetFilesOnly(paths)
                .SelectMany(EnumFiles)
                .Distinct(FileNameComparer)
                .ToImmutableList();

        public static string GetHumanizedFileSize(this string fileName) =>
            GetHumanizedFileSize(GetFileSize(fileName));

        public static string GetHumanizedFileSize(this IEnumerable<string> files) =>
            GetHumanizedFileSize(files.Select(file => file).Sum(GetFileSize));

        public static IEnumerable<string> GetFilesOnly(this IEnumerable<string> paths) =>
            paths
                .Select(path => path.Trim())
                .Where(path => !path.StartsWith("#"))
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(FileNameComparer);

        public static IEnumerable<string> OrderFiles(this IEnumerable<string> files, FilesOrderBy filesOrderBy)
        {
            if (filesOrderBy == FilesOrderBy.None)
            {
                return files;
            }

            var fileInfos = files.Select(file => new FileInfo(file));

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            fileInfos = filesOrderBy switch
            {
                FilesOrderBy.NameAsc => fileInfos.OrderBy(fileInfo => fileInfo.Name, FileNameComparer),
                FilesOrderBy.NameDesc => fileInfos.OrderByDescending(fileInfo => fileInfo.Name, FileNameComparer),
                FilesOrderBy.SizeAsc => fileInfos.OrderBy(fileInfo => fileInfo.Length),
                FilesOrderBy.SizeDesc => fileInfos.OrderByDescending(fileInfo => fileInfo.Length),
                FilesOrderBy.LastWriteTimeAsc => fileInfos.OrderBy(fileInfo => fileInfo.LastWriteTimeUtc),
                FilesOrderBy.LastWriteTimeDesc => fileInfos.OrderByDescending(fileInfo => fileInfo.LastWriteTimeUtc),
                _ => throw new ArgumentOutOfRangeException(nameof(filesOrderBy), filesOrderBy, $"Unknown {filesOrderBy}")
            };

            return fileInfos.Select(fileInfo => fileInfo.FullName);
        }

        private static long GetFileSize(string fileName)
        {
            try
            {
                return new FileInfo(fileName).Length;
            }
            catch (Exception exception)
            {
                CsvDataContextDriver.WriteToLog($"Failed to get {fileName} size", exception);
                return 0;
            }
        }

        private static string GetHumanizedFileSize(long size) =>
            size.Bytes().Humanize("0.#");

        private static CsvParser CreateCsvParser(string fileName, char? csvSeparator, NoBomEncoding noBomEncoding, bool allowComments)
        {
            const int bufferSize = 4096 * 20;

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter = csvSeparator is null,
                AllowComments = allowComments,
                HasHeaderRecord = false,
                DetectColumnCountChanges = false,
                BufferSize = bufferSize,
                ProcessFieldBufferSize = bufferSize
            };

            csvConfiguration.Delimiter = csvSeparator?.ToString() ?? csvConfiguration.Delimiter;

            return new CsvParser(new StreamReader(fileName, NoBomEncodings.Value[noBomEncoding], true, bufferSize / sizeof(char)), csvConfiguration);
        }

        private static IEnumerable<string> EnumFiles(string path)
        {
            try
            {
                // Single file.
                if (File.Exists(path))
                {
                    return new[] { path };
                }

                var file = Path.GetFileName(path);

                string baseDir;

                var isPathOnlyDir = string.IsNullOrWhiteSpace(file) || Directory.Exists(path);
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
                    return Enumerable.Empty<string>();
                }

                return Directory
                    .EnumerateFiles(baseDir, file, file.Contains("**") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch(Exception exception)
            {
                CsvDataContextDriver.WriteToLog($"File enumeration failed for {path}", exception);

                return Enumerable.Empty<string>();
            }
        }

        private static IEnumerable<string[]> CsvReadRows(string fileName, char? csvSeparator, NoBomEncoding noBomEncoding, bool allowComments)
        {
            using var csvParser = CreateCsvParser(fileName, csvSeparator, noBomEncoding, allowComments);

            while (csvParser.Read())
            {
                yield return csvParser.Record;
            }
        }

        [DllImport("kernel32.dll")]
        private static extern int GetSystemDefaultLCID();

        [DllImport("kernel32.dll")]
        private static extern int GetUserDefaultLCID();

        private static IReadOnlyDictionary<NoBomEncoding, Encoding> CalculateNoBomEncodings()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return new Dictionary<NoBomEncoding, Encoding>
            {
                [NoBomEncoding.UTF8]             = Encoding.UTF8,
                [NoBomEncoding.Unicode]          = Encoding.Unicode,
                [NoBomEncoding.BigEndianUnicode] = Encoding.BigEndianUnicode,
                [NoBomEncoding.UTF32]            = Encoding.UTF32,
                [NoBomEncoding.BigEndianUTF32]   = new UTF32Encoding(true, true),
                [NoBomEncoding.ASCII]            = Encoding.ASCII,
                [NoBomEncoding.SystemCodePage]   = GetCodePageEncoding(false),
                [NoBomEncoding.UserCodePage]     = GetCodePageEncoding(true)
            };

            static Encoding GetCodePageEncoding(bool user) =>
                Encoding.GetEncoding(CultureInfo.GetCultureInfo(user ? GetUserDefaultLCID() : GetSystemDefaultLCID()).TextInfo.ANSICodePage);
        }

        private static string? StringIntern(string? str) =>
            str switch
            {
                null => null,
                _ => StringInternCache.TryGetValue(str, out var intern)
                        ? intern
                        : StringInternCache[str] = str
            };

        public static char CsvDetectSeparator(string fileName, string[]? csvData = null)
        {
            var defaultCsvSeparators = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                "tsv" => new[] { '\t', ',', ';' },
                _     => new[] { ',', ';', '\t' }
            };

            var csvSeparator = defaultCsvSeparators.First();

            if (!File.Exists(fileName))
            {
                return csvSeparator;
            }

            var defaultCsvSeparator = csvSeparator;

            try
            {
                // Get most used char from separators as separator.
                csvSeparator = (csvData ?? File.ReadLines(fileName).Take(1))
                    .SelectMany(line => line.ToCharArray())
                    .Where(defaultCsvSeparators.Contains)
                    .GroupBy(ch => ch)
                    .OrderByDescending(chGroup => chGroup.Count())
                    .Select(chGroup => chGroup.Key)
                    .DefaultIfEmpty(csvSeparator)
                    .First();
            }
            catch (Exception exception)
            {
                CsvDataContextDriver.WriteToLog($"CSV separator detection failed for {fileName}", exception);
            }

            if (csvSeparator != defaultCsvSeparator)
            {
                CsvDataContextDriver.WriteToLog($"Using CSV separator '{csvSeparator}' for {fileName}");
            }

            return csvSeparator;
        }
    }
}
