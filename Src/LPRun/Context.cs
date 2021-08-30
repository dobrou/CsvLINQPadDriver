using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using static System.IO.Path;

// ReSharper disable once UnusedMember.Global

namespace LPRun
{
    public static class Context
    {
        private const int MaxSupportedMajorVersion = 5;

        private static readonly string BaseDir = GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)!;

        private static readonly string LpRunDir = GetFullPath("LPRun");
        private static readonly string LpRunExe =
            FrameworkInfo.IsNetFramework
                ? ThrowNotSupportedException("Framework")
                : FrameworkInfo.IsNetNative
                    ? ThrowNotSupportedException("Native")
                    : FrameworkInfo.Version.Major > MaxSupportedMajorVersion
                        ? throw new NotSupportedException($".NET {FrameworkInfo.Version} is not supported. Maximum supported major version is {MaxSupportedMajorVersion}")
                        : $"LPRun6{(FrameworkInfo.Version.Major == 5 ? "-net5" : Environment.Is64BitProcess ? string.Empty : "-x86")}.exe";

        [DoesNotReturn]
        private static string ThrowNotSupportedException(string platform) =>
            throw new NotSupportedException($".NET {platform} is not supported. .NET {platform} version is {FrameworkInfo.Version}");

        private static readonly string ExeDir = GetLpRunFullPath("Bin");
        private static readonly string TemplatesDir = GetLpRunFullPath("Templates");
        private static readonly string DataDir = GetLpRunFullPath("Data");

        public static string FilesDir { get; } = GetLpRunFullPath("Files");
        public static string Exe { get; } = Combine(ExeDir, LpRunExe);

        public static string GetExeFullPath(string path) =>
            Combine(ExeDir, path);

        public static string GetDataFullPath(string path) =>
            Combine(DataDir, path);

        public static string GetTemplatesFullPath(string path) =>
            Combine(TemplatesDir, path);

        public static string GetFilesFullPath(string path) =>
            Combine(FilesDir, path);

        public static string GetFullPath(string path) =>
            Combine(BaseDir, path);

        private static string GetLpRunFullPath(string path) =>
            Combine(LpRunDir, path);
    }
}
