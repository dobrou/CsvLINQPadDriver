using System;
using System.Reflection;

using static System.IO.Path;

// ReSharper disable once UnusedMember.Global

namespace LPRun
{
    public static class Context
    {
        private static readonly string BaseDir = GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)!;
        private static readonly string LpRunDir = GetFullPath("LPRun");

        private static readonly string ExeDir = GetLpRunFullPath("Bin");
        private static readonly string TemplatesDir = GetLpRunFullPath("Templates");
        private static readonly string DataDir = GetLpRunFullPath("Data");

        public static string FilesDir { get; } = GetLpRunFullPath("Files");
        public static string Exe { get; } = Combine(ExeDir, GetLpRunExe());

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

        private static string GetLpRunExe() =>
            Environment.Is64BitProcess
                ? "LPRun6.exe"
                : "LPRun6-x86.exe";
    }
}
