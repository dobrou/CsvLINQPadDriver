using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using static System.IO.Path;

using static LPRun.FrameworkInfo;
using static LPRun.LPRunException;

// ReSharper disable once UnusedMember.Global

namespace LPRun
{
    /// <summary>
    /// Provides collection of methods and properties for accessing the LPRun folders structure.
    /// </summary>
    public static class Context
    {
        private static readonly string BaseDir  = GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)!;

        private static readonly string LpRunDir = GetFullPath("LPRun");
        private static readonly string LpRunExe =
            ThrowNotSupportedExceptionIfNot(IsSupportedCpu, "CPU architecture") ??
            ThrowNotSupportedExceptionIfNot(IsSupportedOs, "OS") ??
            ThrowNotSupportedFrameworkException(IsNetFramework, "Framework") ??
            ThrowNotSupportedFrameworkException(IsNetNative, "Native") ??
            FrameworkInfo.Version.Major switch
            {
                >= 5 and <= 7 or 3 => $"LPRun7-{(IsArm ? "arm" : "x")}{(Is64Bit ? "64" : "86")}.exe",
                _                  => ThrowNotSupportedNetVersionException()
            };

        private static string? ThrowNotSupportedFrameworkException(bool isNotSupported, string platform) =>
            isNotSupported
                ? throw new NotSupportedException($".NET {platform} is not supported. .NET {platform} version is {FrameworkInfo.Version}")
                : null;

        private static string? ThrowNotSupportedExceptionIfNot(bool isSupported, string what) =>
            isSupported
                ? null
                : throw new NotSupportedException($"{what} is not supported");

        [DoesNotReturn]
        private static string ThrowNotSupportedNetVersionException() =>
            throw new NotSupportedException($".NET {FrameworkInfo.Version} is not supported");

        private static readonly string ExeDir       = GetLpRunFullPath("Bin");
        private static readonly string TemplatesDir = GetLpRunFullPath("Templates");
        private static readonly string DataDir      = GetLpRunFullPath("Data");

        /// <summary>
        /// Gets the LPRun\Files folder full path.
        /// </summary>
        /// <returns>The LPRun\Files folder full path.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static string FilesDir { get; } = GetLpRunFullPath("Files");

        /// <summary>
        /// Gets the LPRun\Bin folder full path.
        /// </summary>
        /// <returns>The LPRun\Bin folder full path.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static string Exe { get; } = GetExeFullPath(LpRunExe);

        /// <summary>
        /// Appends the <paramref name="path"/> to the LPRun\Bin path.
        /// </summary>
        /// <param name="path">The path to append to the LPRun\Bin path.</param>
        /// <returns>The <paramref name="path"/> appended to the LPRun\Bin path.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static string GetExeFullPath(string path) =>
            WrapCombine(ExeDir, path);

        /// <summary>
        /// Appends the <paramref name="path"/> to the LPRun\Data path.
        /// </summary>
        /// <param name="path">The path to append to the LPRun\Data path.</param>
        /// <returns>The <paramref name="path"/> appended to the LPRun\Data path.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static string GetDataFullPath(string path) =>
            WrapCombine(DataDir, path);

        /// <summary>
        /// Appends the <paramref name="path"/> to the LPRun\Templates path.
        /// </summary>
        /// <param name="path">The path to append to the LPRun\Templates path.</param>
        /// <returns>The <paramref name="path"/> appended to the LPRun\Templates path.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static string GetTemplatesFullPath(string path) =>
            WrapCombine(TemplatesDir, path);

        /// <summary>
        /// Appends the <paramref name="path"/> to the LPRun\Files path.
        /// </summary>
        /// <param name="path">The path to append to the LPRun\Files path.</param>
        /// <returns>The <paramref name="path"/> appended to the LPRun\Files path.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static string GetFilesFullPath(string path) =>
            WrapCombine(FilesDir, path);

        /// <summary>
        /// Appends the <paramref name="path"/> to the path of executing assembly.
        /// </summary>
        /// <param name="path">The path to append to the path of executing assembly.</param>
        /// <returns>The <paramref name="path"/> appended to the path of executing assembly.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static string GetFullPath(string path) =>
            WrapCombine(BaseDir, path);

        private static string GetLpRunFullPath(string path) =>
            WrapCombine(LpRunDir, path);

        private static string WrapCombine(string path1, string path2) =>
            Wrap(() => Combine(path1, path2));
    }
}
