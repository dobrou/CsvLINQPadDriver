using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text.RegularExpressions;

namespace CsvLINQPadDriver.Helpers
{
    internal class CodeGenHelper
    {
        private const string safeChar = "_";
        private const int maxLength = 128;

        private static readonly Regex codeNameInvalidCharacters = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]", RegexOptions.Compiled);

        private static readonly string[] invalidIdentifierNames = { "System", "ToString", "Equals", "GetHashCode" };
        private static readonly Lazy<CodeDomProvider> csCodeProvider = new Lazy<CodeDomProvider>(() => CodeDomProvider.CreateProvider("C#"));

        public static string GetSafeCodeName(string name)
        {
            string safeName = name ?? "";

            if (safeName.Length > maxLength)
                safeName = safeName.Substring(0, maxLength);

            safeName = codeNameInvalidCharacters.Replace(safeName, safeChar);
            safeName = Regex.Replace(safeName, safeChar + "+", safeChar);
            safeName = Regex.Replace(safeName, "^" + safeChar + "+", "");

            if (string.IsNullOrEmpty(safeName))
                return safeChar + "empty";

            if (!char.IsLetter(safeName, 0))
                safeName = safeChar + safeName;

            if (!csCodeProvider.Value.IsValidIdentifier(safeName) || invalidIdentifierNames.Contains(safeName))
                safeName += safeChar;

            return safeName;
        }
    }
}
