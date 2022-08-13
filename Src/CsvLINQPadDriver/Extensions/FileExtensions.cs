using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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

        public static readonly StringComparer FileNameComparer = StringComparer.OrdinalIgnoreCase;

        private const string RecursiveMaskMarker = "**";

        private const StringComparison FileNameComparison = StringComparison.OrdinalIgnoreCase;

        private static readonly UnicodeCharsetDetector.UnicodeCharsetDetector UnicodeCharsetDetector = new();

        private static readonly char[] TsvSeparators   = "\t,;".ToCharArray();
        private static readonly char[] CsvSeparators   = ",;\t".ToCharArray();
        private static readonly char[] WhiteSpaceChars = " \t" .ToCharArray();

        private static readonly char[] InlineCommentCharArray = InlineComment.ToCharArray();

        private static
#if NETCOREAPP
            HashSet<string>?
#else
            Dictionary<string, string>?
#endif
                _stringInternCache;

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
                .Where(static supportedFileType => !string.IsNullOrWhiteSpace(supportedFileType.Extension))
                .Select(static supportedFileType => $".{supportedFileType.Extension}"),
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
            SupportedFileTypes.FirstOrDefault(supportedFileType => supportedFileType.FileType == fileType) ?? throw new IndexOutOfRangeException($"Unknown {nameof(FileType)} {fileType}");

        public static readonly string Filter = string.Join("|", SupportedFileTypes.Select(static supportedFileType => $"{supportedFileType.Description} Files (*.{supportedFileType.Mask})|*.{supportedFileType.Mask}"));

        public static IEnumerable<T> CsvReadRows<T>(
            this string fileName,
            string? csvSeparator,
            bool internString,
            StringComparer? internStringComparer,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            bool doNotLockFiles,
            bool addHeader,
            HeaderDetection? headerDetection,
            WhitespaceTrimOptions? whitespaceTrimOptions,
            bool allowSkipLeadingRows,
            int skipLeadingRowsCount,
            CsvRowMappingBase<T> csvClassMap)
            where T : ICsvRowBase, new()
        {
            _stringInternCache ??= internStringComparer is null
                ? new()
                : new(internStringComparer);

            return
                SkipHeader(
                    CsvReadRows(
                        fileName,
                        csvSeparator,
                        noBomEncoding,
                        allowComments,
                        commentChar,
                        ignoreBadData,
                        autoDetectEncoding,
                        ignoreBlankLines,
                        doNotLockFiles,
                        whitespaceTrimOptions,
                        allowSkipLeadingRows,
                        skipLeadingRowsCount))
                .Select(GetRecord);

            IEnumerable<string[]> SkipHeader(IEnumerable<string[]> rows)
            {
                using var enumerator = rows.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    var columnHeader = enumerator.Current;

                    if (addHeader && !columnHeader.IsPresent(true, headerDetection ?? default))
                    {
                        yield return columnHeader;
                    }
                }

                while(enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }

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
            string? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            bool doNotLockFiles,
            bool addHeader,
            HeaderDetection headerDetection,
            HeaderFormat headerFormat,
            WhitespaceTrimOptions? whitespaceTrimOptions,
            bool allowSkipLeadingRows,
            int skipLeadingRowsCount)
        {
            using var csvParser = CreateCsvParser(
                fileName,
                csvSeparator,
                noBomEncoding,
                allowComments,
                commentChar,
                ignoreBadData,
                autoDetectEncoding,
                ignoreBlankLines,
                doNotLockFiles,
                whitespaceTrimOptions,
                allowSkipLeadingRows,
                skipLeadingRowsCount);

            return csvParser.Read()
                    ? GetHeader()
                    : Array.Empty<string>();

            string[] GetHeader()
            {
                var header = csvParser.Record;
                var headerFormatFunc = GetHeaderFormatFunc();

                if (!header!.IsPresent(addHeader, headerDetection))
                {
                    return
                        addHeader
                            ? Enumerable
                                .Range(0, header!.Length)
                                .Select(headerFormatFunc)
                                .ToArray()
                            : header!;
                }

                var getUniqueFallbackColumnNameFunc = GetUniqueFallbackColumnNameGenerator();

                return AdjustColumnNames();

                string[] AdjustColumnNames()
                {
                    for (var i = 0; i < header!.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(header[i]))
                        {
                            header[i] = getUniqueFallbackColumnNameFunc();
                        }
                    }

                    return header;
                }

                Func<int, string> GetHeaderFormatFunc()
                {
                    var name = Enum.GetName(typeof(HeaderFormat), headerFormat);
                    if (name is null)
                    {
                        throw new IndexOutOfRangeException($"Unknown {nameof(HeaderFormat)} {headerFormat}");
                    }

                    var columnName = name[..^1];
                    var startIndex = name[^1] == '0' ? 0 : 1;
                    var format = $"{columnName}{{0}}";

                    return i => string.Format(CultureInfo.InvariantCulture, format, i + startIndex);
                }

                Func<string> GetUniqueFallbackColumnNameGenerator()
                {
                    var enumerator = Enumerable.Range(0, int.MaxValue).GetEnumerator();
                    var lookup = new Lazy<HashSet<string>>(() => new HashSet<string>(header!), LazyThreadSafetyMode.None);

                    return GetUniqueFallbackColumnName;

                    string GetUniqueFallbackColumnName()
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            var columnName = headerFormatFunc(current);
                            if (lookup.Value.Add(columnName))
                            {
                                return columnName;
                            }
                        }

                        throw new InvalidOperationException("Unexpected error occurred");
                    }
                }
            }
        }

        public static char CsvDetectSeparator(this string fileName, bool doNotLockFiles, bool debugInfo)
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
                csvSeparator = GetHeaderChars()
                    .Where(defaultCsvSeparators.Contains)
                    .GroupBy(static ch => ch)
                    .OrderByDescending(static chGroup => chGroup.Count())
                    .Select(static chGroup => chGroup.Key)
                    .DefaultIfEmpty(csvSeparator)
                    .First();

                IEnumerable<char> GetHeaderChars()
                {
                    using var streamReader = new StreamReader(OpenFile(fileName, doNotLockFiles));
                    return streamReader.ReadLine()?.ToCharArray() ?? Array.Empty<char>();
                }
            }
            catch (Exception exception) when (exception.CanBeHandled())
            {
                $"CSV separator detection failed for {fileName}".WriteToLog(debugInfo, exception);
            }

            if (csvSeparator != defaultCsvSeparator)
            {
                $"Using CSV separator '{csvSeparator}' for {fileName}".WriteToLog(debugInfo);
            }

            return csvSeparator;
        }

        public static bool IsCsvFormatValid(
            this string fileName,
            string? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            bool doNotLockFiles,
            bool debugInfo,
            WhitespaceTrimOptions whitespaceTrimOptions,
            bool allowSkipLeadingRows,
            int skipLeadingRowsCount)
        {
            var header = $"{fileName} is not valid CSV file:";

            if (!File.Exists(fileName))
            {
                $"{header} file does not exist".WriteToLog(debugInfo);

                return false;
            }

            try
            {
                using var csvParser = CreateCsvParser(
                    fileName,
                    csvSeparator,
                    noBomEncoding,
                    allowComments,
                    commentChar,
                    ignoreBadData,
                    autoDetectEncoding,
                    ignoreBlankLines,
                    doNotLockFiles,
                    whitespaceTrimOptions,
                    allowSkipLeadingRows,
                    skipLeadingRowsCount);

                if (!csvParser.Read())
                {
                    $"{header} could not get CSV header".WriteToLog(debugInfo);

                    return false;
                }

                var headerRow = csvParser.Record;

                // No columns.
                if (!headerRow!.Any())
                {
                    $"{header} CSV header had no columns".WriteToLog(debugInfo);

                    return false;
                }

                if (!csvParser.Read())
                {
                    $"{header} CSV has header but has no data".WriteToLog(debugInfo);

                    return false;
                }

                var dataRow = csvParser.Record;

                // Column count differs.
                if (headerRow!.Length != dataRow!.Length)
                {
                    $"{header} CSV header column count does not match data column count".WriteToLog(debugInfo);

                    return false;
                }

                // Too many strange characters.
                var charsCount = headerRow
                    .Concat(dataRow)
                    .Sum(static s => s?.Length ?? 0);

                var validCharsCount = headerRow
                    .Concat(dataRow)
                    .Sum(static s => Enumerable.Range(0, s?.Length ?? 0).Count(i => char.IsLetterOrDigit(s ?? string.Empty, i)));

                const double validCharsMinOkRatio = 0.5;

                return validCharsCount >= validCharsMinOkRatio * charsCount;
            }
            catch(Exception exception) when (exception.CanBeHandled())
            {
                $"{header} failed with exception".WriteToLog(debugInfo, exception);

                return false;
            }
        }

        public static string GetLongestCommonPrefixPath(this IEnumerable<string> paths)
        {
            var pathsValid = paths.GetFiles().ToImmutableList();

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
            paths
                .GetFiles()
                .SelectMany(files => EnumFiles(files, exceptions))
                .Distinct(FileNameComparer)
                .ToImmutableList();

        public static string GetHumanizedFileSize(this string fileName, bool debugInfo) =>
            GetHumanizedFileSize(GetFileSize(fileName, debugInfo));

        public static string GetHumanizedFileSize(this IEnumerable<string> files, bool debugInfo) =>
            GetHumanizedFileSize(files.Sum(fileName => GetFileSize(fileName, debugInfo)));

        public static IEnumerable<string> GetFiles(this string paths) =>
            Regex.Split(paths, @"[\r\n]+").GetFiles();

        private static IEnumerable<string> GetFiles(this IEnumerable<string> paths) =>
            paths
                .Select(static path => path.Trim())
                .Where(static path => !path.IsInlineComment())
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Distinct(FileNameComparer);

        public static IEnumerable<string> OrderFiles(this IEnumerable<string> files, FilesOrderBy filesOrderBy)
        {
            if (filesOrderBy == FilesOrderBy.None)
            {
                return files;
            }

            var fileInfos = files.Select(static file => new FileInfo(file));

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            fileInfos = filesOrderBy switch
            {
                FilesOrderBy.NameAsc           => fileInfos.OrderBy(static fileInfo => fileInfo.Name, FileNameComparer),
                FilesOrderBy.NameDesc          => fileInfos.OrderByDescending(static fileInfo => fileInfo.Name, FileNameComparer),
                FilesOrderBy.SizeAsc           => fileInfos.OrderBy(static fileInfo => fileInfo.Length),
                FilesOrderBy.SizeDesc          => fileInfos.OrderByDescending(static fileInfo => fileInfo.Length),
                FilesOrderBy.LastWriteTimeAsc  => fileInfos.OrderBy(static fileInfo => fileInfo.LastWriteTimeUtc),
                FilesOrderBy.LastWriteTimeDesc => fileInfos.OrderByDescending(static fileInfo => fileInfo.LastWriteTimeUtc),
                _                              => throw new IndexOutOfRangeException($"Unknown {nameof(FilesOrderBy)} {filesOrderBy}")
            };

            return fileInfos.Select(static fileInfo => fileInfo.FullName);
        }

        public static string GetInlineCommentContent(this string line) =>
            line.Trim().TrimStart(InlineCommentCharArray);

        private static long GetFileSize(string fileName, bool debugInfo)
        {
            try
            {
                return new FileInfo(fileName).Length;
            }
            catch (Exception exception) when (exception.CanBeHandled())
            {
                $"Failed to get {fileName} size".WriteToLog(debugInfo, exception);
                return 0;
            }
        }

        private static string GetHumanizedFileSize(long size) =>
            size.Bytes().Humanize("0.#");

        private static CsvParser CreateCsvParser(
            string fileName,
            string? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            bool doNotLockFiles,
            WhitespaceTrimOptions? whitespaceTrimOptions,
            bool allowSkipLeadingRows,
            int skipLeadingRowsCount)
        {
            const int bufferSize = 4096 * 20;

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter          = csvSeparator is null,
                AllowComments            = allowComments,
                HasHeaderRecord          = false,
                DetectColumnCountChanges = false,
                IgnoreBlankLines         = ignoreBlankLines,
                TrimOptions              = GetTrimOptions(),
                BufferSize               = bufferSize,
                ProcessFieldBufferSize   = bufferSize
            };

            csvConfiguration.Delimiter    = csvSeparator ?? csvConfiguration.Delimiter;
            csvConfiguration.Comment      = commentChar ?? csvConfiguration.Comment;
            csvConfiguration.BadDataFound = ignoreBadData ? null! : csvConfiguration.BadDataFound;

            var whiteSpaceChars = WhiteSpaceChars
                .Except(csvConfiguration.Delimiter.Length == 1
                            ? new[] { csvConfiguration.Delimiter.First() }
                            : Array.Empty<char>())
                .ToArray();

            if (whiteSpaceChars.Any())
            {
                csvConfiguration.WhiteSpaceChars = whiteSpaceChars;
            }

            var encoding = (autoDetectEncoding ? DetectEncoding(fileName) : null) ?? GetFallbackEncoding(noBomEncoding);

            var csvParser = new CsvParser(new StreamReader(OpenFile(fileName, doNotLockFiles), encoding, !autoDetectEncoding, bufferSize / sizeof(char)), csvConfiguration);

            SkipLeadingRows();

            return csvParser;

            TrimOptions GetTrimOptions() =>
                whitespaceTrimOptions switch
                {
                    null                               => TrimOptions.None,
                    WhitespaceTrimOptions.All          => TrimOptions.Trim | TrimOptions.InsideQuotes,
                    WhitespaceTrimOptions.Trim         => TrimOptions.Trim,
                    WhitespaceTrimOptions.InsideQuotes => TrimOptions.InsideQuotes,
                    _                                  => throw new IndexOutOfRangeException($"Unknown {nameof(WhitespaceTrimOptions)} {whitespaceTrimOptions}")
                };

            void SkipLeadingRows()
            {
                if (!allowSkipLeadingRows)
                {
                    return;
                }

                while (skipLeadingRowsCount-- > 0 && csvParser.Read())
                {
                    // Skip row.
                }
            }
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
            string? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            bool doNotLockFiles,
            WhitespaceTrimOptions? whitespaceTrimOptions,
            bool allowSkipLeadingRows,
            int skipLeadingRowsCount)
        {
            using var csvParser = CreateCsvParser(
                fileName,
                csvSeparator,
                noBomEncoding,
                allowComments,
                commentChar,
                ignoreBadData,
                autoDetectEncoding,
                ignoreBlankLines,
                doNotLockFiles,
                whitespaceTrimOptions,
                allowSkipLeadingRows,
                skipLeadingRowsCount);

            while (csvParser.Read())
            {
                yield return csvParser.Record!;
            }
        }

        private static bool IsPresent(this IEnumerable<string> header, bool addHeader, HeaderDetection headerDetection)
        {
            return
                addHeader &&
                TryGetHeaderDetectionRegex(headerDetection, out var headerDetectionRegex) &&
                header.All(columnHeader => string.IsNullOrWhiteSpace(columnHeader) || Regex.IsMatch(columnHeader, headerDetectionRegex, RegexOptions.ExplicitCapture));

            static bool TryGetHeaderDetectionRegex(HeaderDetection headerDetection, [NotNullWhen(true)] out string? headerDetectionRegex) =>
                (headerDetectionRegex = headerDetection switch
                {
                    HeaderDetection.NoHeader                       => null,
                    HeaderDetection.HasHeader                      => @".",
                    HeaderDetection.AllLettersNumbersPunctuation   => @"^\p{L}\p{M}*(\p{L}\p{M}*|[0-9_\-. ])*$",
                    HeaderDetection.AllLettersNumbers              => @"^\p{L}\p{M}*(\p{L}\p{M}*|[0-9])*$",
                    HeaderDetection.AllLetters                     => @"^(\p{L}\p{M}*)+$",
                    HeaderDetection.LatinLettersNumbersPunctuation => @"^[a-zA-Z][a-zA-Z0-9_\-. ]*$",
                    HeaderDetection.LatinLettersNumbers            => @"^[a-zA-Z][a-zA-Z0-9]*$",
                    HeaderDetection.LatinLetters                   => @"^[a-zA-Z]+$",
                    _                                              => throw new IndexOutOfRangeException($"Unknown {nameof(HeaderDetection)} {headerDetection}")
                }) is not null;
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
                    NoBomEncoding.UTF8             => Encoding.UTF8,
                    NoBomEncoding.Unicode          => Encoding.Unicode,
                    NoBomEncoding.BigEndianUnicode => Encoding.BigEndianUnicode,
                    NoBomEncoding.UTF32            => Encoding.UTF32,
                    NoBomEncoding.BigEndianUTF32   => new UTF32Encoding(true, true),
#pragma warning disable SYSLIB0001
                    NoBomEncoding.UTF7             => Encoding.UTF7,
#pragma warning restore SYSLIB0001
                    NoBomEncoding.ASCII            => Encoding.ASCII,
                    NoBomEncoding.SystemCodePage   => GetCodePageEncoding(false),
                    NoBomEncoding.UserCodePage     => GetCodePageEncoding(true),
                    _                              => Encoding.GetEncoding(FromCodePage())
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

                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                            _                                             => null
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

            if (_stringInternCache!.TryGetValue(str, out var intern))
            {
                return intern;
            }

            _stringInternCache.Add(str
#if !NETCOREAPP
                , str
#endif
            );

            return str;
        }

        private static bool IsInlineComment(this string line) =>
            line.TrimStart().StartsWith(InlineComment);

        private static Stream OpenFile(string fileName, bool doNotLockFiles) =>
            new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                doNotLockFiles
                    ? FileShare.ReadWrite
                    : FileShare.Read);
    }
}
