using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text.RegularExpressions;

namespace CsvLINQPadDriver.Helpers
{
    internal class CodeGenHelper
    {
        private const string SafeChar = "_";
        private const int MaxLength = 128;

        private static readonly Lazy<CodeDomProvider> CodeDomProvider = new Lazy<CodeDomProvider>(() => System.CodeDom.Compiler.CodeDomProvider.CreateProvider("C#"));
        private static readonly Regex CodeNameInvalidCharacters = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

        private static readonly string[] InvalidIdentifierNames = { "System", nameof(ToString), nameof(Equals), nameof(GetHashCode) };

        public static string GetSafeCodeName(string name)
        {
            var safeName = name ?? string.Empty;

            if (safeName.Length > MaxLength)
            {
                safeName = safeName.Substring(0, MaxLength);
            }

            safeName = CodeNameInvalidCharacters.Replace(safeName, SafeChar);
            safeName = Regex.Replace(safeName, $"{SafeChar}+", SafeChar);
            safeName = Regex.Replace(safeName, $"^{SafeChar}+", string.Empty);

            if (string.IsNullOrEmpty(safeName))
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
