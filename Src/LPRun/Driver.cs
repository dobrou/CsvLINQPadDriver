using System;
using System.IO;
using System.Linq;

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
    /// Provides method for the LINQPad driver installation.
    /// </summary>
    public static class Driver
    {
        /// <summary>
        /// Installs the LINQPad driver and related driver files.
        /// </summary>
        /// <param name="driverDir">The directory to copy driver <paramref name="files"/> to.</param>
        /// <param name="files">The LINQPad driver files.</param>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static void Install(string driverDir, params string[] files)
        {
            Wrap(Execute);

            void Execute()
            {
                var driverPath = Combine(GetExeFullPath(@"drivers\DataContext\NetCore"), driverDir);

                CreateDirectory(driverPath);

                if (!files.Any())
                {
                    throw new ArgumentException("At least one file should be specified", nameof(files));
                }

                Array.ForEach(files, CopyFile);

                void CopyFile(string file) =>
                ExecIfFileIsNewer(file, (srcFile, dstFile) => Copy(srcFile, dstFile, true));

                void ExecIfFileIsNewer(string file, Action<string, string> action)
                {
                    var srcFile = Path.GetFullPath(file);
                    var dstFile = Combine(driverPath, GetFileName(file));

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
}
