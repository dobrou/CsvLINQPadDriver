using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CsvLINQPadDriver.Extensions
{
    internal static class CodeGenExtensions
    {
        private static readonly Lazy<CodeDomProvider> CsCodeDomProvider = new(static () => CodeDomProvider.CreateProvider("C#"));
        private static readonly string[] InvalidIdentifierNames = { nameof(System), nameof(ToString), nameof(Equals), nameof(GetHashCode) };

        public static string GetSafeCodeName(this string? name)
        {
            const string safeChar = "_";
            const int maxLength = 128;

            var safeName = name ?? string.Empty;

            safeName = Replaces().Aggregate(safeName[..Math.Min(safeName.Length, maxLength)], Replace);

            if (string.IsNullOrWhiteSpace(safeName))
            {
                return $"{safeChar}empty";
            }

            if (!char.IsLetter(safeName, 0))
            {
                safeName = safeChar + safeName;
            }

            return !CsCodeDomProvider.Value.IsValidIdentifier(safeName) || InvalidIdentifierNames.Contains(safeName)
                ? safeName + safeChar
                : safeName;

            static string Replace(string input, (string Pattern, string Replacement) replace) =>
                Regex.Replace(input, replace.Pattern, replace.Replacement);

            static IEnumerable<(string Pattern, string Replacement)> Replaces()
            {
                yield return (@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]", safeChar);
                yield return ($"{safeChar}+", safeChar);
                yield return ($"^{safeChar}|{safeChar}$", string.Empty);
            }
        }
    }
}
