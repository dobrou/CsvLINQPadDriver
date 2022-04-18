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
        /// <param name="file">The LINQPad script file.</param>
        /// <param name="connection">The LINQPad script connection header created by call to <see cref="ConnectionHeader"/>.<see cref="ConnectionHeader.Get{T}"/> method.</param>
        /// <returns>The LINQPad script file created by combining the script <paramref name="file"/> and the <paramref name="connection"/> header.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
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
