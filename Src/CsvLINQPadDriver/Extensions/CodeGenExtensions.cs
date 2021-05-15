using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text.RegularExpressions;

namespace CsvLINQPadDriver.Extensions
{
    internal static class CodeGenExtensions
    {
        private const string SafeChar = "_";
        private const int MaxLength = 128;

        private static readonly Lazy<CodeDomProvider> CodeDomProvider = new(() => System.CodeDom.Compiler.CodeDomProvider.CreateProvider("C#"));
        private static readonly Regex CodeNameInvalidCharacters = new(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

        private static readonly string[] InvalidIdentifierNames = { nameof(System), nameof(ToString), nameof(Equals), nameof(GetHashCode) };

        public static string GetSafeCodeName(this string? name)
        {
            var safeName = name ?? string.Empty;

            if (safeName.Length > MaxLength)
            {
                safeName = safeName[..MaxLength];
            }

            safeName = CodeNameInvalidCharacters.Replace(safeName, SafeChar);
            safeName = Regex.Replace(safeName, $"{SafeChar}+", SafeChar);
            safeName = Regex.Replace(safeName, $"^{SafeChar}+", string.Empty);

            if (string.IsNullOrWhiteSpace(safeName))
            {
                return $"{SafeChar}empty";
            }

            if (!char.IsLetter(safeName, 0))
            {
                safeName = SafeChar + safeName;
            }

            if (!CodeDomProvider.Value.IsValidIdentifier(safeName) || InvalidIdentifierNames.Contains(safeName))
            {
                safeName += SafeChar;
            }

            return safeName;
        }
    }
}
