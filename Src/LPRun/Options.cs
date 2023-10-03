using System;

// ReSharper disable UnusedMember.Global

namespace LPRun;

/// <summary>
/// Defines LPRun command-line options.
/// </summary>
/// <seealso href="https://www.linqpad.net/lprun.aspx">LINQPad Command-Line and Scripting</seealso>
public static class Options
{
    /// <summary>
    /// Defines LPRun command-line compilation options.
    /// </summary>
    public static class Compilation
    {
        /// <summary>
        /// Switch enables compiler optimizations. This incurs the usual trade-off: slightly faster execution with compute-intensive code in exchange for less accurate error reporting.
        /// </summary>
        public const string Optimize        = "-optimize";

        /// <summary>
        /// Switch outputs compiler warnings. Warnings are written to stderr <see cref="Console.Error"/>.
        /// </summary>
        /// <remarks>As warnings are written to stderr LPRun execution will fail in case of any.</remarks>
        public const string OutputWarnings  = "-warn";

        /// <summary>
        /// Switch tells LPRun to check that the query will compile, without actually running anything.
        /// </summary>
        public const string CompileOnly     = "-compileonly";
    }
}
