using System.Text.RegularExpressions;

using static System.Text.RegularExpressions.RegexOptions;

namespace CsvLINQPadDriver.Extensions
{
    internal static partial class FileExtensions
    {
        private const string GetFilesRegexPattern                                      = @"[\r\n]+";
        private const string MatchDeduceIsFileOrFolderRegexPattern                     = @"[\\/]$";
        private const string DeduceIsFileOrFolderRegexPattern                          = @"[?*]";

        private const RegexOptions HeaderDetectionRegexOptions                         = ExplicitCapture | Compiled;

        private const string HeaderDetectionHasHeaderRegexPattern                      = @".";
        private const string HeaderDetectionAllLettersNumbersPunctuationRegexPattern   = @"^\p{L}\p{M}*(\p{L}\p{M}*|[0-9_\-. ])*$";
        private const string HeaderDetectionAllLettersNumbersRegexPattern              = @"^\p{L}\p{M}*(\p{L}\p{M}*|[0-9])*$";
        private const string HeaderDetectionAllLettersRegexPattern                     = @"^(\p{L}\p{M}*)+$";
        private const string HeaderDetectionLatinLettersNumbersPunctuationRegexPattern = @"^[a-zA-Z][a-zA-Z0-9_\-. ]*$";
        private const string HeaderDetectionLatinLettersNumbersRegexPattern            = @"^[a-zA-Z][a-zA-Z0-9]*$";
        private const string HeaderDetectionLatinLettersRegexPattern                   = @"^[a-zA-Z]+$";

#if NET7_0_OR_GREATER
        [GeneratedRegex(GetFilesRegexPattern, None, Config.Regex.TimeoutMs)]
        private static partial Regex GetFilesRegex();

        [GeneratedRegex(MatchDeduceIsFileOrFolderRegexPattern, None, Config.Regex.TimeoutMs)]
        private static partial Regex MatchDeduceIsFileOrFolderRegex();

        [GeneratedRegex(DeduceIsFileOrFolderRegexPattern, None, Config.Regex.TimeoutMs)]
        private static partial Regex DeduceIsFileOrFolderRegex();

        [GeneratedRegex(HeaderDetectionHasHeaderRegexPattern, HeaderDetectionRegexOptions, Config.Regex.TimeoutMs)]
        private static partial Regex HeaderDetectionHasHeaderRegex();

        [GeneratedRegex(HeaderDetectionAllLettersNumbersPunctuationRegexPattern, HeaderDetectionRegexOptions, Config.Regex.TimeoutMs)]
        private static partial Regex HeaderDetectionAllLettersNumbersPunctuationRegex();

        [GeneratedRegex(HeaderDetectionAllLettersNumbersRegexPattern, HeaderDetectionRegexOptions, Config.Regex.TimeoutMs)]
        private static partial Regex HeaderDetectionAllLettersNumbersRegex();

        [GeneratedRegex(HeaderDetectionAllLettersRegexPattern, HeaderDetectionRegexOptions, Config.Regex.TimeoutMs)]
        private static partial Regex HeaderDetectionAllLettersRegex();

        [GeneratedRegex(HeaderDetectionLatinLettersNumbersPunctuationRegexPattern, HeaderDetectionRegexOptions, Config.Regex.TimeoutMs)]
        private static partial Regex HeaderDetectionLatinLettersNumbersPunctuationRegex();

        [GeneratedRegex(HeaderDetectionLatinLettersNumbersRegexPattern, HeaderDetectionRegexOptions, Config.Regex.TimeoutMs)]
        private static partial Regex HeaderDetectionLatinLettersNumbersRegex();

        [GeneratedRegex(HeaderDetectionLatinLettersRegexPattern, HeaderDetectionRegexOptions, Config.Regex.TimeoutMs)]
        private static partial Regex HeaderDetectionLatinLettersRegex();
#else
        private static readonly Regex GetFilesRegexVar = new(GetFilesRegexPattern, Compiled, Config.Regex.Timeout);
        private static Regex GetFilesRegex() => GetFilesRegexVar;

        private static readonly Regex MatchDeduceIsFileOrFolderRegexVar = new(MatchDeduceIsFileOrFolderRegexPattern, Compiled, Config.Regex.Timeout);
        private static Regex MatchDeduceIsFileOrFolderRegex() => MatchDeduceIsFileOrFolderRegexVar;

        private static readonly Regex DeduceIsFileOrFolderRegexVar = new(DeduceIsFileOrFolderRegexPattern, Compiled, Config.Regex.Timeout);
        private static Regex DeduceIsFileOrFolderRegex() => DeduceIsFileOrFolderRegexVar;

        private static readonly Regex HeaderDetectionHasHeaderRegexVar = new(HeaderDetectionHasHeaderRegexPattern, HeaderDetectionRegexOptions, Config.Regex.Timeout);
        private static Regex HeaderDetectionHasHeaderRegex() => HeaderDetectionHasHeaderRegexVar;

        private static readonly Regex HeaderDetectionAllLettersNumbersPunctuationRegexVar = new(HeaderDetectionAllLettersNumbersPunctuationRegexPattern, HeaderDetectionRegexOptions, Config.Regex.Timeout);
        private static Regex HeaderDetectionAllLettersNumbersPunctuationRegex() => HeaderDetectionAllLettersNumbersPunctuationRegexVar;

        private static readonly Regex HeaderDetectionAllLettersNumbersRegexVar = new(HeaderDetectionAllLettersNumbersRegexPattern, HeaderDetectionRegexOptions, Config.Regex.Timeout);
        private static Regex HeaderDetectionAllLettersNumbersRegex() => HeaderDetectionAllLettersNumbersRegexVar;

        private static readonly Regex HeaderDetectionAllLettersRegexVar = new(HeaderDetectionAllLettersRegexPattern, HeaderDetectionRegexOptions, Config.Regex.Timeout);
        private static Regex HeaderDetectionAllLettersRegex() => HeaderDetectionAllLettersRegexVar;

        private static readonly Regex HeaderDetectionLatinLettersNumbersPunctuationRegexVar = new(HeaderDetectionLatinLettersNumbersPunctuationRegexPattern, HeaderDetectionRegexOptions, Config.Regex.Timeout);
        private static Regex HeaderDetectionLatinLettersNumbersPunctuationRegex() => HeaderDetectionLatinLettersNumbersPunctuationRegexVar;

        private static readonly Regex HeaderDetectionLatinLettersNumbersRegexVar = new(HeaderDetectionLatinLettersNumbersRegexPattern, HeaderDetectionRegexOptions, Config.Regex.Timeout);
        private static Regex HeaderDetectionLatinLettersNumbersRegex() => HeaderDetectionLatinLettersNumbersRegexVar;

        private static readonly Regex HeaderDetectionLatinLettersRegexVar = new(HeaderDetectionLatinLettersRegexPattern, HeaderDetectionRegexOptions, Config.Regex.Timeout);
        private static Regex HeaderDetectionLatinLettersRegex() => HeaderDetectionLatinLettersRegexVar;
#endif
    }
}
