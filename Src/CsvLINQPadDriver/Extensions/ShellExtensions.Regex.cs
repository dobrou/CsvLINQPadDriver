using System.Text.RegularExpressions;

using static System.Text.RegularExpressions.RegexOptions;

namespace CsvLINQPadDriver.Extensions;

internal static partial class ShellExtensions
{
    private const string SelectFileRegexPattern = @"\s+\([^)]+?\)$";

#if NET7_0_OR_GREATER
    [GeneratedRegex(SelectFileRegexPattern, None, Config.Regex.TimeoutMs)]
    private static partial Regex SelectFileRegex();
#else
    private static readonly Regex SelectFileRegexVar = new(SelectFileRegexPattern, Compiled, Config.Regex.Timeout);
    private static Regex SelectFileRegex() => SelectFileRegexVar;
#endif
}
