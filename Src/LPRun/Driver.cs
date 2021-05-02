using System;
using System.IO;
using System.Linq;

namespace LPRun
{
    public static class Driver
    {
        public static void Install(string driverDir, params string[] files)
        {
            var driverPath = Path.Combine(Context.GetExeFullPath(@"drivers\DataContext\NetCore"), driverDir);

            Directory.CreateDirectory(driverPath);

            if (!files.Any())
            {
                throw new ArgumentException("At least one file should be specified", nameof(files));
            }

            Array.ForEach(files, CopyFile);

            void CopyFile(string file) =>
                ExecIfFileIsNewer(file, (srcFile, dstFile) => File.Copy(srcFile, dstFile, true));

            void ExecIfFileIsNewer(string file, Action<string, string> action)
            {
                var srcFile = Path.GetFullPath(file);
                var dstFile = Path.Combine(driverPath, Path.GetFileName(file));

                var srcFileInfo = new FileInfo(srcFile);
                var dstFileInfo = new FileInfo(dstFile);

                if (!dstFileInfo.Exists || dstFileInfo.LastWriteTime < srcFileInfo.LastWriteTime)
                {
                    action(srcFile, dstFile);
                }
            }
        }
    }
}
