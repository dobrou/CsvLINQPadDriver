using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using static System.IO.Path;
using static LPRun.FrameworkInfo;

// ReSharper disable once UnusedMember.Global

namespace LPRun
{
    public static class Context
    {
        private static readonly string BaseDir = GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)!;

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
