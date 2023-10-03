using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using LPRun;

namespace CsvLINQPadDriverTest;

[TestFixture]
public sealed partial class LPRunTests
{
#if GITHUB_ACTIONS
    private const int RetryCount = 3;
#endif
    private static readonly TimeSpan RetryTimeout = TimeSpan.FromMinutes(2);

    private sealed record FileEncoding(string FileName, Encoding Encoding);

    [OneTimeSetUp]
    public void Init()
    {
        const string driverFileName = "CsvLINQPadDriver";

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        Driver.EnsureNotInstalledViaNuGet(driverFileName);
        Driver.InstallWithDepsJson(driverFileName, $"{driverFileName}.dll", "Tests");

        CreateFileEncodings(@"Encoding\Utf8Cp65001",   Encoding.Default);
        CreateFileEncodings(@"Encoding\German\Cp1252", Encoding.GetEncoding("Windows-1252"));

        static void CreateFileEncodings(string baseFile, Encoding encoding)
        {
            var directory = Path.GetDirectoryName(baseFile);
            var content = File.ReadAllText(GetFilePath(baseFile), encoding);

            Array.ForEach(GetFileEncodings().ToArray(), WriteFiles);

            static IEnumerable<FileEncoding> GetFileEncodings()
            {
                yield return new("Utf16BomCp1200", Encoding.Unicode);
                yield return new("Utf16BomCp1201", Encoding.BigEndianUnicode);
                yield return new("Utf8BomCp65001", Encoding.UTF8);
                yield return new("Utf32Bom",       Encoding.UTF32);
            }

            void WriteFiles(FileEncoding fileEncoding) =>
                File.WriteAllText(GetFilePath(fileEncoding.FileName), content, fileEncoding.Encoding);

            string GetFilePath(string fileName) =>
                Context.GetDataFullPath(Path.Combine(directory!, $"{Path.GetFileName(fileName)}.csv"));
        }
    }

    private static Task<Runner.Result> ExecuteAsync(string scriptFile) =>
        Runner.ExecuteAsync(scriptFile,
            RetryTimeout
#if GITHUB_ACTIONS
            , new (RetryCount)
#endif
        );
}
