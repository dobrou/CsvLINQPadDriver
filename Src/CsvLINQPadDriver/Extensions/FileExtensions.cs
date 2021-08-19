using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

using Humanizer;

using UnicodeCharsetDetector;

using CsvHelper;
using CsvHelper.Configuration;

using CsvLINQPadDriver.CodeGen;

#if NETCOREAPP
using System.Collections.Immutable;
#else
using CsvLINQPadDriver.Bcl.Extensions;
#endif

namespace CsvLINQPadDriver.Extensions
{
    internal static class FileExtensions
    {
        public const string InlineComment = "#";

        private const string RecursiveMaskMarker = "**";

        private const StringComparison FileNameComparison = StringComparison.OrdinalIgnoreCase;

        private static readonly UnicodeCharsetDetector.UnicodeCharsetDetector UnicodeCharsetDetector = new();

        private static readonly char[] TsvSeparators = "\t,;".ToCharArray();
        private static readonly char[] CsvSeparators = ",;\t".ToCharArray();

        private static readonly char[] InlineCommentCharArray = InlineComment.ToCharArray();

        private static readonly StringComparer FileNameComparer = StringComparer.OrdinalIgnoreCase;

        private static
#if NETCOREAPP
            HashSet<string>
#else
            Dictionary<string, string>
#endif
                StringInternCache = null!;

        private static readonly Dictionary<NoBomEncoding, Encoding> NoBomEncodings = new();

        private record SupportedFileType(FileType FileType, string Extension, string Description)
        {
            private readonly string? _mask;

            public string Mask
            {
                get => _mask ?? Extension;
                init => _mask = value;
            }
        }

        private static readonly SupportedFileType[] SupportedFileTypes =
        {
            new ( FileType.CSV,  "csv", "CSV"  ),
            new ( FileType.TSV,  "tsv", "TSV"  ),
            new ( FileType.Text, "txt", "Text" ),
            new ( FileType.Log,  "log", "Log"  ),
            new ( FileType.All,  "",    "All"  ) { Mask = "*" }
        };

        private static readonly HashSet<string> SupportedFileExtensions = new(
            SupportedFileTypes
                .Where(supportedFileType => !string.IsNullOrWhiteSpace(supportedFileType.Extension))
                .Select(supportedFileType => $".{supportedFileType.Extension}"),
            FileNameComparer);

        private static readonly FileType DefaultFileType = SupportedFileTypes.First().FileType;
        private static readonly string DefaultMask = GetMask(DefaultFileType);

#if NETCOREAPP
        static FileExtensions() =>
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif

        public static string GetMask(this FileType fileType, bool recursive = false) =>
            $"{(recursive ? RecursiveMaskMarker : "*")}.{fileType.GetSupportedFileType().Mask}";

        public static string GetExtension(this FileType fileType) =>
            fileType.GetSupportedFileType().Extension;

        public static int GetFilterIndex(this FileType fileType) =>
            Array.FindIndex(SupportedFileTypes, supportedFileType => supportedFileType.FileType == fileType) + 1;

        private static SupportedFileType GetSupportedFileType(this FileType fileType) =>
            SupportedFileTypes.FirstOrDefault(supportedFileType => supportedFileType.FileType == fileType) ?? throw new ArgumentException($"Unknown {fileType}", nameof(fileType));

        public static readonly string Filter = string.Join("|", SupportedFileTypes.Select(supportedFileType => $"{supportedFileType.Description} Files (*.{supportedFileType.Mask})|*.{supportedFileType.Mask}"));

        public static IEnumerable<T> CsvReadRows<T>(
            this string fileName,
            char? csvSeparator,
            bool internString,
            StringComparer? internStringComparer,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            WhitespaceTrimOptions whitespaceTrimOptions,
            CsvRowMappingBase<T> csvClassMap)
            where T : ICsvRowBase, new()
        {
            StringInternCache = internStringComparer is null
                ? new()
                : new(internStringComparer);

            return CsvReadRows(fileName, csvSeparator, noBomEncoding, allowComments, commentChar, ignoreBadData, autoDetectEncoding, ignoreBlankLines, whitespaceTrimOptions)
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

        public static IEnumerable<string> CsvReadHeader(
            this string fileName,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            WhitespaceTrimOptions whitespaceTrimOptions)
        {
            using var csvParser = CreateCsvParser(fileName, csvSeparator, noBomEncoding, allowComments, commentChar, ignoreBadData, autoDetectEncoding, ignoreBlankLines, whitespaceTrimOptions);

            return csvParser.Read()
                    ? csvParser.Record
                    : Array.Empty<string>();
        }

        public static char CsvDetectSeparator(this string fileName, string[]? csvData = null)
        {
            var defaultCsvSeparators = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".tsv" => TsvSeparators,
                _      => CsvSeparators
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
            catch (Exception exception) when (exception.CanBeHandled())
            {
                CsvDataContextDriver.WriteToLog($"CSV separator detection failed for {fileName}", exception);
            }

            if (csvSeparator != defaultCsvSeparator)
            {
                CsvDataContextDriver.WriteToLog($"Using CSV separator '{csvSeparator}' for {fileName}");
            }

            return csvSeparator;
        }

        public static bool IsCsvFormatValid(
            this string fileName,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            WhitespaceTrimOptions whitespaceTrimOptions)
        {
            var header = $"{fileName} is not valid CSV file:";

            if (!File.Exists(fileName))
            {
                CsvDataContextDriver.WriteToLog($"{header} file does not exist");

                return false;
            }

            try
            {
                using var csvParser = CreateCsvParser(fileName, csvSeparator, noBomEncoding, allowComments, commentChar, ignoreBadData, autoDetectEncoding, ignoreBlankLines, whitespaceTrimOptions);

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
            catch(Exception exception) when (exception.CanBeHandled())
            {
                CsvDataContextDriver.WriteToLog($"{header} failed with exception", exception);

                return false;
            }
        }

        public static string GetLongestCommonPrefixPath(this IEnumerable<string> paths)
        {
            var pathsValid = paths.GetFilesOnly().ToImmutableList();

            // Get longest common path prefix.
            var filePaths = pathsValid.FirstOrDefault()?.Split(Path.DirectorySeparatorChar) ?? Array.Empty<string>();

            var directorySeparator = Path.DirectorySeparatorChar
#if !NETCOREAPP
                .ToString()
#endif
            ;

            return Enumerable.Range(1, filePaths.Length)
                .Select(i => string.Join(directorySeparator, filePaths.Take(i).ToImmutableList()))
                .LastOrDefault(prefix => pathsValid.All(path => path.StartsWith(prefix, FileNameComparison))) ?? string.Empty;
        }

        public static IEnumerable<string> EnumFiles(this IEnumerable<string> paths, ICollection<Exception>? exceptions = null) =>
            GetFilesOnly(paths)
                .SelectMany(files => EnumFiles(files, exceptions))
                .Distinct(FileNameComparer)
                .ToImmutableList();

        public static string GetHumanizedFileSize(this string fileName) =>
            GetHumanizedFileSize(GetFileSize(fileName));

        public static string GetHumanizedFileSize(this IEnumerable<string> files) =>
            GetHumanizedFileSize(files.Select(file => file).Sum(GetFileSize));

        public static IEnumerable<string> GetFilesOnly(this string paths) =>
            Regex.Split(paths, @"[\r\n]+").GetFilesOnly();

        private static IEnumerable<string> GetFilesOnly(this IEnumerable<string> paths) =>
            paths
                .Select(path => path.Trim())
                .Where(path => !path.IsInlineComment())
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

        public static string GetInlineCommentContent(this string line) =>
            line.Trim().TrimStart(InlineCommentCharArray);

        private static long GetFileSize(string fileName)
        {
            try
            {
                return new FileInfo(fileName).Length;
            }
            catch (Exception exception) when (exception.CanBeHandled())
            {
                CsvDataContextDriver.WriteToLog($"Failed to get {fileName} size", exception);
                return 0;
            }
        }

        private static string GetHumanizedFileSize(long size) =>
            size.Bytes().Humanize("0.#");

        private static CsvParser CreateCsvParser(
            string fileName,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            WhitespaceTrimOptions whitespaceTrimOptions)
        {
            const int bufferSize = 4096 * 20;

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter = csvSeparator is null,
                AllowComments = allowComments,
                HasHeaderRecord = false,
                DetectColumnCountChanges = false,
                IgnoreBlankLines = ignoreBlankLines,
                TrimOptions = GetTrimOptions(),
                BufferSize = bufferSize,
                ProcessFieldBufferSize = bufferSize
            };

            csvConfiguration.Delimiter = csvSeparator?.ToString() ?? csvConfiguration.Delimiter;
            csvConfiguration.Comment = commentChar ?? csvConfiguration.Comment;
            csvConfiguration.BadDataFound = ignoreBadData ? null : csvConfiguration.BadDataFound;

            var encoding = (autoDetectEncoding ? DetectEncoding(fileName) : null) ?? GetFallbackEncoding(noBomEncoding);

            return new CsvParser(new StreamReader(fileName, encoding, !autoDetectEncoding, bufferSize / sizeof(char)), csvConfiguration);

            TrimOptions GetTrimOptions() =>
                whitespaceTrimOptions switch
                {
                    WhitespaceTrimOptions.None => TrimOptions.None,
                    WhitespaceTrimOptions.Trim => TrimOptions.Trim,
                    WhitespaceTrimOptions.InsideQuotes => TrimOptions.InsideQuotes,
                    _ => throw new ArgumentException($"Unknown trim option: {whitespaceTrimOptions}", nameof(whitespaceTrimOptions))
                };
        }

        public record DeduceFileOrFolderResult(bool IsFile, string Path);

        public static DeduceFileOrFolderResult DeduceIsFileOrFolder(this string path, bool removeMask = false) =>
            Regex.IsMatch(path, @"[\\/]$")
                ? new DeduceFileOrFolderResult(false, path)
                : Regex.IsMatch(Path.GetFileName(path), @"[?*]")
                    ? new DeduceFileOrFolderResult(true, removeMask ? Path.GetDirectoryName(path) ?? path : path)
                    : new DeduceFileOrFolderResult(SupportedFileExtensions.Contains(Path.GetExtension(path)), path);

        public static void Add(this ICollection<Exception>? exceptions, string file, Exception exception) =>
            exceptions?.Add(file, $"processing failed: {exception.Message}");

        public static void Add(this ICollection<Exception>? exceptions, string file, string message) =>
            exceptions?.Add(new Exception($"'{file}' {message}".AppendDot()));

        private static IEnumerable<string> EnumFiles(string path, ICollection<Exception>? exceptions = null)
        {
            try
            {
                // Single file.
                if (File.Exists(path))
                {
                    return new[] { path };
                }

                var fileOrMask = Path.GetFileName(path);

                string baseDir;

                var isPathOnlyDir = string.IsNullOrWhiteSpace(fileOrMask) || Directory.Exists(path);
                if (isPathOnlyDir)
                {
                    fileOrMask = DefaultMask;
                    baseDir = path;
                }
                else
                {
                    baseDir = Path.GetDirectoryName(path) ?? string.Empty;
                }

                return
#if NETCOREAPP
                    Directory
#else
                    exceptions
#endif
                        .EnumerateFiles(baseDir, fileOrMask,
                            Path.GetFileNameWithoutExtension(fileOrMask).Contains(RecursiveMaskMarker)
                                ? SearchOption.AllDirectories
                                : SearchOption.TopDirectoryOnly)
                        .SkipExceptions(exceptions);
            }
            catch (Exception exception) when (exception.CanBeHandled())
            {
                exceptions.Add(path, exception);

                return Enumerable.Empty<string>();
            }
        }

        private static IEnumerable<string[]> CsvReadRows(
            string fileName,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            WhitespaceTrimOptions whitespaceTrimOptions)
        {
            using var csvParser = CreateCsvParser(fileName, csvSeparator, noBomEncoding, allowComments, commentChar, ignoreBadData, autoDetectEncoding, ignoreBlankLines, whitespaceTrimOptions);

            while (csvParser.Read())
            {
                yield return csvParser.Record;
            }
        }

        private static Encoding GetFallbackEncoding(NoBomEncoding noBomEncoding)
        {
            if (!NoBomEncodings.TryGetValue(noBomEncoding, out var encoding))
            {
                NoBomEncodings.Add(noBomEncoding, encoding = GetEncoding());
            }

            return encoding;

            Encoding GetEncoding()
            {
                return noBomEncoding switch
                {
                    NoBomEncoding.UTF8 => Encoding.UTF8,
                    NoBomEncoding.Unicode => Encoding.Unicode,
                    NoBomEncoding.BigEndianUnicode => Encoding.BigEndianUnicode,
                    NoBomEncoding.UTF32 => Encoding.UTF32,
                    NoBomEncoding.BigEndianUTF32 => new UTF32Encoding(true, true),
                    NoBomEncoding.UTF7 => Encoding.UTF7,
                    NoBomEncoding.ASCII => Encoding.ASCII,
                    NoBomEncoding.SystemCodePage => GetCodePageEncoding(false),
                    NoBomEncoding.UserCodePage => GetCodePageEncoding(true),
                    _ => Encoding.GetEncoding(FromCodePage())
                };

                static Encoding GetCodePageEncoding(bool user) =>
                    Encoding.GetEncoding(CultureInfo.GetCultureInfo(user ? GetUserDefaultLCID() : GetSystemDefaultLCID()).TextInfo.ANSICodePage);

                int FromCodePage() =>
                    Convert.ToInt32(noBomEncoding.ToString()[2..], CultureInfo.InvariantCulture);

                [DllImport("kernel32.dll")]
                static extern int GetSystemDefaultLCID();

                [DllImport("kernel32.dll")]
                static extern int GetUserDefaultLCID();
            }
        }

        private static Encoding? DetectEncoding(string fileName)
        {
            try
            {
                Charset charset;

                using (var stream = File.OpenRead(fileName))
                {
                    charset = UnicodeCharsetDetector.Check(stream);
                }

                return DetectAsciiEncoding() ?? charset.ToEncoding();

                Encoding? DetectAsciiEncoding()
                {
                    try
                    {
                        return charset switch
                        {
                            Charset.None or Charset.Ansi or Charset.Ascii => UtfUnknown.CharsetDetector.DetectFromFile(fileName).Detected?.Encoding,
                            _ => null
                        };
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static string? StringIntern(string? str)
        {
            if (str is null)
            {
                return null;
            }

            if (StringInternCache.TryGetValue(str, out var intern))
            {
                return intern;
            }

#if NETCOREAPP
            StringInternCache.Add(str);
#else
            StringInternCache.Add(str, str);
#endif

            return str;
        }

        private static bool IsInlineComment(this string line) =>
            line.TrimStart().StartsWith(InlineComment);
    }
}
