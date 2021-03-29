using System;
using System.IO;
using System.Reflection;

namespace CsvLINQPadDriverTest.LPRun
{
    internal static class Context
    {
        private static readonly string BaseDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase!).LocalPath)!;
        private static readonly string LpRunDir = GetFullPath("LPRun");

        private static readonly string ExeDir = GetLpRunFullPath("Bin");
        private static readonly string TemplatesDir = GetLpRunFullPath("Templates");
        private static readonly string CsvDir = GetLpRunFullPath("CSV");

        public static string FilesDir { get; } = GetLpRunFullPath("Files");
        public static string Exe { get; } = Path.Combine(ExeDir, GetLpRunExe());

        public static string GetExeFullPath(string path) =>
            Path.Combine(ExeDir, path);

        public static string GetCsvFullPath(string path) =>
            Path.Combine(CsvDir, path);

        public static string GetTemplatesFullPath(string path) =>
            Path.Combine(TemplatesDir, path);

        public static string GetFilesFullPath(string path) =>
            Path.Combine(FilesDir, path);

        public static string GetFullPath(string path) =>
            Path.Combine(BaseDir, path);

        private static string GetLpRunFullPath(string path) =>
            Path.Combine(LpRunDir, path);

        private static string GetLpRunExe() =>
            Environment.Is64BitProcess
                ? "LPRun6.exe"
                : "LPRun6-x86.exe";
    }
}
