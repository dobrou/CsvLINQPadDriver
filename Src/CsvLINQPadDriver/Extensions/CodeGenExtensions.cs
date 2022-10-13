using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CsvLINQPadDriver.Extensions
{
    internal static partial class CodeGenExtensions
    {
        private static readonly Lazy<CodeDomProvider> CsCodeDomProvider = new(static () => CodeDomProvider.CreateProvider("C#"));
        private static readonly string[] InvalidIdentifierNames = { nameof(System), nameof(ToString), nameof(Equals), nameof(GetHashCode) };

        private const string SafeChar = "_";

        public static string GetSafeCodeName(this string? name)
        {
            const int maxLength = 128;

            var safeName = name ?? string.Empty;

            safeName = Replaces().Aggregate(safeName[..Math.Min(safeName.Length, maxLength)], Replace);

            if (string.IsNullOrWhiteSpace(safeName))
            {
                return $"{SafeChar}empty";
            }

            if (!char.IsLetter(safeName, 0))
            {
                safeName = SafeChar + safeName;
            }

            return !CsCodeDomProvider.Value.IsValidIdentifier(safeName) || InvalidIdentifierNames.Contains(safeName)
                ? safeName + SafeChar
                : safeName;

            static string Replace(string input, (Regex Regex, string Replacement) replace) =>
                replace.Regex.Replace(input, replace.Replacement);

            static IEnumerable<(Regex Regex, string Replacement)> Replaces()
            {
                yield return (ReplaceRegex1(), SafeChar);
                yield return (ReplaceRegex2(), SafeChar);
                yield return (ReplaceRegex3(), string.Empty);
            }
        }
    }
}
