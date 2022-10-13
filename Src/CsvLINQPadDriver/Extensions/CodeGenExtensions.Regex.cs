using System.Text.RegularExpressions;

using static System.Text.RegularExpressions.RegexOptions;

namespace CsvLINQPadDriver.Extensions
{
    internal static partial class CodeGenExtensions
    {
        private const string ReplaceRegexPattern1 = @"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]";
        private const string ReplaceRegexPattern2 = $"{SafeChar}+";
        private const string ReplaceRegexPattern3 = $"^{SafeChar}|{SafeChar}$";

#if NET7_0_OR_GREATER
        [GeneratedRegex(ReplaceRegexPattern1, None, Config.Regex.TimeoutMs)]
        private static partial Regex ReplaceRegex1();

        [GeneratedRegex(ReplaceRegexPattern2, None, Config.Regex.TimeoutMs)]
        private static partial Regex ReplaceRegex2();

        [GeneratedRegex(ReplaceRegexPattern3, None, Config.Regex.TimeoutMs)]
        private static partial Regex ReplaceRegex3();
#else
        private static readonly Regex ReplaceRegex1Var = new(ReplaceRegexPattern1, Compiled, Config.Regex.Timeout);
        private static Regex ReplaceRegex1() => ReplaceRegex1Var;

        private static readonly Regex ReplaceRegex2Var = new(ReplaceRegexPattern2, Compiled, Config.Regex.Timeout);
        private static Regex ReplaceRegex2() => ReplaceRegex2Var;

        private static readonly Regex ReplaceRegex3Var = new(ReplaceRegexPattern3, Compiled, Config.Regex.Timeout);
        private static Regex ReplaceRegex3() => ReplaceRegex3Var;
#endif
    }
}
