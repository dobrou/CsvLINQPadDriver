using System;

using static System.Environment;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;

using static LPRun.Context;
using static LPRun.LPRunException;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace LPRun;

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
    /// <param name="scriptFileName">The expected script file name without extension. <paramref name="file"/> by default.</param>
    /// <returns>The path to LINQPad script file created by combining the script <paramref name="file"/> content and the <paramref name="connection"/> header.</returns>
    /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
    /// <example>
    /// This shows how to create the LINQPad script file and get path to it:
    /// <code>
    /// var pathToLinqScript = LinqScript.FromFile(
    ///     // The LINQPad script file which content will be appended to connection header.
    ///     $"{linqScriptName}.linq",
    ///     // The LINQPad script connection header created by call to ConnectionHeader.Get method.
    ///     connectionHeader);
    /// );
    /// </code>
    /// </example>
    /// <seealso cref="ConnectionHeader.Get{T}"/>
    public static string FromFile(string file, string connection, string? scriptFileName = null)
    {
        return Wrap(Execute);

        string Execute()
        {
            file = GetTemplatesFullPath(file);
            var outFile = GetFileName(scriptFileName is not null ? $"{scriptFileName}{GetExtension(file)}" : file);

            return WriteScript(outFile, connection, ReadAllText(file));
        }
    }

    /// <summary>
    /// Creates the LINQPad script file by combining the <paramref name="script"/> content and the <paramref name="connection"/> header.
    /// </summary>
    /// <param name="script">The LINQPad script which content will be appended to <paramref name="connection"/> header.</param>
    /// <param name="connection">The LINQPad script connection header created by call to <see cref="ConnectionHeader"/>.<see cref="ConnectionHeader.Get{T}"/> method.</param>
    /// <param name="scriptFileName">The expected script file name without extension. If <see langword="null" /> or empty the script file name will be generated. Your are in charge of removing that file afterwards.</param>
    /// <returns>The path to LINQPad script file created by combining the script <paramref name="script"/> content and the <paramref name="connection"/> header.</returns>
    /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
    /// <example>
    /// This shows how to create the LINQPad script file and get path to it:
    /// <code>
    /// var pathToLinqScript = LinqScript.FromScript(
    ///     // The LINQPad script content which will be appended to connection header.
    ///     @"""Hello, world!""",
    ///     // The minimal script connection header.
    ///     @"&lt;Query Kind=""Expression"" /&gt;");
    /// );
    /// </code>
    /// </example>
    /// <seealso cref="ConnectionHeader.Get{T}"/>
    public static string FromScript(string script, string connection, string? scriptFileName = null)
    {
        return Wrap(Execute);

        string Execute()
        {
            if (string.IsNullOrEmpty(scriptFileName))
            {
                scriptFileName = $"Script_{Guid.NewGuid().ToString()[^12..]}"; // e.g. Script_92083af85e6d
            }

            var file = ChangeExtension(scriptFileName, ".linq");

            return WriteScript(file, connection, script);
        }
    }

    private static string WriteScript(string file, string connection, string content)
    {
        CreateDirectory(FilesDir);

        var outFile = GetFilesFullPath(file);

        using var textWriter = CreateText(outFile);

        textWriter.Write(connection);
        if (!connection.EndsWith(NewLine))
        {
            textWriter.WriteLine();
            textWriter.WriteLine();
        }
        textWriter.Write(content);

        return outFile;
    }

    /// <summary>
    /// Creates the LINQPad script file by combining the script <paramref name="file"/> content and the <paramref name="connection"/> header.
    /// </summary>
    /// <param name="file">The LINQPad script file which content will be appended to <paramref name="connection"/> header.</param>
    /// <param name="connection">The LINQPad script connection header created by call to <see cref="ConnectionHeader"/>.<see cref="ConnectionHeader.Get{T}"/> method.</param>
    /// <param name="scriptFileName">The expected script file name without extension. <paramref name="file"/> by default.</param>
    /// <returns>The path to LINQPad script file created by combining the script <paramref name="file"/> content and the <paramref name="connection"/> header.</returns>
    /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
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
    /// <seealso cref="ConnectionHeader.Get{T}"/>
#pragma warning disable S1133
    [Obsolete($"Please use {nameof(FromFile)} method instead.")]
#pragma warning restore S1133
    public static string Create(string file, string connection, string? scriptFileName = null) =>
        FromFile(file, connection, scriptFileName);
}
