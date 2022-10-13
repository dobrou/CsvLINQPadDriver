using System.Text.RegularExpressions;

using static System.Text.RegularExpressions.RegexOptions;

namespace CsvLINQPadDriver
{
    internal partial class ConnectionDialog
    {
        private const string AppendFilesRegexPattern = @"[\r\n]$";

#if NET7_0_OR_GREATER
        [GeneratedRegex(AppendFilesRegexPattern, None, Config.Regex.TimeoutMs)]
        private static partial Regex AppendFilesRegex();
#else
        private static readonly Regex AppendFilesRegexVar = new (AppendFilesRegexPattern, Compiled, Config.Regex.Timeout);
        private static Regex AppendFilesRegex() => AppendFilesRegexVar;
#endif
    }
}
