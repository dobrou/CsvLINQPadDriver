using System;
using System.IO;
using System.Linq;

using static System.Environment;
using static System.Environment.SpecialFolder;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;

using static LPRun.Context;
using static LPRun.LPRunException;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace LPRun;

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
    /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
    /// <example>
    /// This shows how to install the LINQPad driver and specify the driver dependencies JSON (use InstallWithDepsJson instead):
    /// <code>
    /// Driver.Install(
    ///     // The directory to copy driver files to.
    ///     "CsvLINQPadDriver",
    ///     // The LINQPad driver files.
    ///     "CsvLINQPadDriver.dll",
    ///     Context.GetDepsJsonRelativePath("CsvLINQPadDriver", "Tests")
    /// );
    /// </code>
    /// </example>
    /// <seealso cref="GetDepsJsonRelativePath(string, string)"/>
    /// <seealso cref="GetDepsJsonRelativePath(string, Func{string, string})"/>
    /// <seealso cref="InstallWithDepsJson(string, string, string, string[])"/>
    /// <seealso cref="InstallWithDepsJson(string, string, Func{string, string}, string[])"/>
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

    /// <summary>
    /// Installs the LINQPad driver with the driver dependencies JSON and related driver files.
    /// </summary>
    /// <param name="driverDir">The directory to copy driver <paramref name="files"/> to.</param>
    /// <param name="driverFileName">The directory to copy driver <paramref name="files"/> to.</param>
    /// <param name="testsFolderPath">The test folder path which resides into the driver build folder.</param>
    /// <param name="files">The LINQPad driver files.</param>
    /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
    /// <example>
    /// This shows how to install the LINQPad driver with the driver dependencies JSON:
    /// <code>
    /// Driver.Install(
    ///     // The directory to copy driver files to.
    ///     "CsvLINQPadDriver",
    ///     // The LINQPad driver files.
    ///     "CsvLINQPadDriver.dll",
    ///     // The test folder path.
    ///     "Tests"
    /// );
    /// </code>
    /// </example>
    public static void InstallWithDepsJson(string driverDir, string driverFileName, string testsFolderPath, params string[] files) =>
        Install(driverDir, files.Concat(new[] { driverFileName, GetDepsJsonRelativePath(driverFileName, testsFolderPath) }).ToArray());

    /// <summary>
    /// Installs the LINQPad driver with the driver dependencies JSON and related driver files.
    /// </summary>
    /// <param name="driverDir">The directory to copy driver <paramref name="files"/> to.</param>
    /// <param name="driverFileName">The directory to copy driver <paramref name="files"/> to.</param>
    /// <param name="getDepsJsonFileFullPath">The function which returns the absolute driver dependencies JSON path based on the tests build folder path.</param>
    /// <param name="files">The LINQPad driver files.</param>
    /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
    /// <example>
    /// This shows how to install the LINQPad driver with the driver dependencies JSON (this does the same thing as overloaded method):
    /// <code>
    /// Driver.Install(
    ///     // The directory to copy driver files to.
    ///     "CsvLINQPadDriver",
    ///     // The LINQPad driver files.
    ///     "CsvLINQPadDriver.dll",
    ///     // The function which returns the absolute driver dependencies JSON path.
    ///     baseDir => baseDir.Replace("Tests", string.Empty, StringComparison.OrdinalIgnoreCase)
    /// );
    /// </code>
    /// </example>
    public static void InstallWithDepsJson(string driverDir, string driverFileName, Func<string, string> getDepsJsonFileFullPath, params string[] files) =>
        Install(driverDir, files.Concat(new[] { driverFileName, GetDepsJsonRelativePath(driverFileName, getDepsJsonFileFullPath) }).ToArray());

    /// <summary>
    /// Ensures that there is no driver is installed via NuGet. Throws <see cref="LPRunException"/> exception otherwise.
    /// </summary>
    /// <param name="driverDir">The driver directory. The file search patterns are allowed; it might be useful for looking for signed drivers.</param>
    /// <exception cref="LPRunException">Thrown when driver has been installed via NuGet.</exception>
    /// <example>
    /// This shows how to verify that driver is not installed via NuGet:
    /// <code>
    /// Driver.EnsureNotInstalledViaNuGet("CsvLINQPadDriver*");
    /// </code>
    /// </example>
    public static void EnsureNotInstalledViaNuGet(string driverDir)
    {
        Wrap(CheckForDriverDir);

        void CheckForDriverDir()
        {
            try
            {
                var path = $@"{GetFolderPath(LocalApplicationData)}\LINQPad\NuGet.Drivers";

                if (GetDirectories(path, driverDir).Any())
                {
                    throw new LPRunException($@"""{driverDir}"" has been installed via NuGet under ""{path}"". LPRun will use it instead of local one. Please uninstall the driver via LINQPad or delete it manually. You can also remove call of {nameof(EnsureNotInstalledViaNuGet)} if this is intended");
                }
            }
            catch (Exception e) when(e is not LPRunException)
            {
                // Ignore other exceptions.
            }
        }
    }
}
