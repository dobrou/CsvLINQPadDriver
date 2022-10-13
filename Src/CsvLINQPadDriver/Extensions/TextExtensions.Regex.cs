using System.Text.RegularExpressions;

using static System.Text.RegularExpressions.RegexOptions;

namespace CsvLINQPadDriver.Extensions
{
    internal static partial class TextExtensions
    {
        private const string AppendDotRegexPattern = @"\p{P}\s*$";

#if NET7_0_OR_GREATER
        [GeneratedRegex(AppendDotRegexPattern, None, Config.Regex.TimeoutMs)]
        private static partial Regex AppendDotRegex();
#else
        private static readonly Regex AppendDotRegexVar = new(AppendDotRegexPattern, Compiled, Config.Regex.Timeout);
        private static Regex AppendDotRegex() => AppendDotRegexVar;
#endif
    }
}
