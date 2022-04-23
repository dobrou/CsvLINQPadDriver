using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;

using static LPRun.Context;
using static LPRun.LPRunException;

// ReSharper disable UnusedType.Global
// ReSharper disable once UnusedMember.Global

namespace LPRun
{
    /// <summary>
    /// Provides method for for creating the LINQPad script by combining script file content and connection header.
    /// </summary>
    public static class LinqScript
    {
        /// <summary>
        /// Creates the LINQPad script file by combining the script <paramref name="file"/> content and the <paramref name="connection"/> header.
        /// </summary>
        /// <param name="file">The LINQPad script file which content will be appended to <paramref name="connection"/> header.</param>
        /// <param name="connection">The LINQPad script connection header created by call to <see cref="ConnectionHeader"/>.<see cref="ConnectionHeader.Get{T}"/> method.</param>
        /// <returns>The path to LINQPad script file created by combining the script <paramref name="file"/> content and the <paramref name="connection"/> header.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        /// <example>
        /// This shows how to create the LINQPad script file and get path to it:
        /// <code>
        /// var pathToLinqScript = LinqScript.Create(
        ///     // The LINQPad script file which content will be appended to connection header.
        ///     $"{linqScriptName}.linq",
        ///     // The LINQPad script connection header created by call to ConnectionHeader.Get method.
        ///     connectionHeader);
        /// );
        /// </code>
        /// </example>
        public static string Create(string file, string connection)
        {
            return Wrap(Execute);

            string Execute()
            {
                CreateDirectory(FilesDir);

                file = GetTemplatesFullPath(file);

                var outFile = GetFilesFullPath(GetFileName(file));

                using var textWriter = CreateText(outFile);

                textWriter.Write(connection);
                textWriter.Write(ReadAllText(file));

                return outFile;
            }
        }
    }
}
