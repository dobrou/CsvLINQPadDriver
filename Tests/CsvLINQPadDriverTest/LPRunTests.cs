using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Moq;

using NUnit.Framework;

using CsvLINQPadDriver;
using CsvLINQPadDriverTest.LPRun;

namespace CsvLINQPadDriverTest
{
    [TestFixture]
    public class LPRunTests
    {
        private static readonly string Files = Context.GetCsvFullPath("**.csv");

        [OneTimeSetUp]
        public void Init()
        {
            Driver.Install("CsvLINQPadDriver",
                "CsvLINQPadDriver.dll",
                "CsvHelper.dll",
                "Humanizer.dll"
            );

            CreateEncodingFiles();

            static void CreateEncodingFiles()
            {
                var contents = File.ReadAllText(GetFilePath("Utf8Cp65001"), Encoding.Default);

                Array.ForEach(GetEncodings().ToArray(), fileEncoding => File.WriteAllText(GetFilePath(fileEncoding.FileName), contents, fileEncoding.Encoding));

                static IEnumerable<(string FileName, Encoding Encoding)> GetEncodings()
                {
                    yield return ("Utf16BomCp1200", Encoding.Unicode);
                    yield return ("Utf16BomCp1201", Encoding.BigEndianUnicode);
                    yield return ("Utf8BomCp65001", Encoding.UTF8);
                    yield return ("Utf32Bom",       Encoding.UTF32);
                }

                static string GetFilePath(string fileName) =>
                    Context.GetCsvFullPath(Path.Combine("Encoding", $"{fileName}.csv"));
            }
        }

        [Test]
        [TestCaseSource(nameof(TestsData))]
        public void Execute_ScriptWithDriverProperties_Success((string linqScriptName, string? context, ICsvDataContextDriverProperties driverProperties) testData)
        {
            var (linqScriptName, context, driverProperties) = testData;

            var queryConfig = GetQueryHeaders().Aggregate(new StringBuilder(), (stringBuilder, h) =>
            {
                stringBuilder.AppendLine(h);
                stringBuilder.AppendLine();
                return stringBuilder;
            }).ToString();

            var linqScript = LinqScript.Create($"{linqScriptName}.linq", queryConfig);

            Console.Write($"{linqScript}{Environment.NewLine}{Environment.NewLine}{queryConfig}");

            var (output, error, exitCode) = Runner.Execute(linqScript, TimeSpan.FromMinutes(2));

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine(output);
            }

            error.Should().BeNullOrWhiteSpace();
            exitCode.Should().Be(0);

            IEnumerable<string> GetQueryHeaders()
            {
                yield return ConnectionHeader.Get("CsvLINQPadDriver", "CsvLINQPadDriver.CsvDataContextDriver", driverProperties!, "System.Runtime.CompilerServices");
                yield return @"string Reason([CallerLineNumber] int sourceLineNumber = 0) => $""something went wrong at line #{sourceLineNumber}"";";
                if (!string.IsNullOrWhiteSpace(context))
                {
                    yield return $"var context = {context};";
                }
            }
        }

        private static IEnumerable<(string linqScriptName, string? context, ICsvDataContextDriverProperties driverProperties)> TestsData()
        {
            const string? noContext = null;
            const StringComparison defaultStringComparison = StringComparison.InvariantCulture;

            var defaultCsvDataContextDriverProperties = GetDefaultCsvDataContextDriverPropertiesObject(defaultStringComparison);

            var linqScriptNames = new[]
            {
                "Generation",
                "Relations"
            };

            var multipleDriverPropertiesTestData = GetCsvDataContextDriverProperties()
                .SelectMany(driverProperties =>
                    linqScriptNames.Select(linqScriptName =>
                        (linqScriptName, 
                         $"new {{ {nameof(driverProperties.UseSingleClassForSameFiles)} = {driverProperties.UseSingleClassForSameFiles.ToString().ToLowerInvariant()} }}",
                         driverProperties)));

            var singleDriverPropertiesTestData = new[] { "Extensions", "SimilarFilesRelations" }
                .Select(linqFile =>
                    (linqFile, 
                     noContext,
                     defaultCsvDataContextDriverProperties));

            var stringComparisonDriverPropertiesTestData = ((StringComparison[])Enum.GetValues(typeof(StringComparison)))
                .SelectMany(stringComparison =>
                    new [] { "StringComparison", "Encoding" }.Select(linqFile =>
                        (linqFile,
                         $"new {{ {nameof(defaultCsvDataContextDriverProperties.StringComparison)} = {nameof(StringComparison)}.{stringComparison} }}",
                         GetDefaultCsvDataContextDriverPropertiesObject(stringComparison))));

            var allowCommentsTestData = new[] { true, false }
                .Select(allowComments =>
                        ("Comments",
                         $"new {{ ExpectedCount = {(allowComments ? 1 : 2)} }}",
                         GetDefaultCsvDataContextDriverPropertiesObject(defaultStringComparison, allowComments)));

            return multipleDriverPropertiesTestData
                    .Concat(singleDriverPropertiesTestData!)
                    .Concat(allowCommentsTestData!)
                    .Concat(stringComparisonDriverPropertiesTestData)!;

            IEnumerable<ICsvDataContextDriverProperties> GetCsvDataContextDriverProperties()
            {
                yield return defaultCsvDataContextDriverProperties;

                yield return Mock.Of<ICsvDataContextDriverProperties>(csvDataContextDriverProperties =>
                    csvDataContextDriverProperties.Files == Files &&
                    csvDataContextDriverProperties.DebugInfo &&
                    csvDataContextDriverProperties.DetectRelations &&
                    csvDataContextDriverProperties.UseSingleClassForSameFiles == false &&
                    csvDataContextDriverProperties.StringComparison == defaultStringComparison &&
                    csvDataContextDriverProperties.IsStringInternEnabled == false &&
                    csvDataContextDriverProperties.IgnoreInvalidFiles == false &&
                    csvDataContextDriverProperties.IsCacheEnabled == false &&
                    csvDataContextDriverProperties.HideRelationsFromDump == false &&
                    csvDataContextDriverProperties.Persist == false);
            }

            static ICsvDataContextDriverProperties GetDefaultCsvDataContextDriverPropertiesObject(StringComparison stringComparison, bool allowComments = false) =>
                Mock.Of<ICsvDataContextDriverProperties>(csvDataContextDriverProperties =>
                    csvDataContextDriverProperties.Files == Files &&
                    csvDataContextDriverProperties.DebugInfo &&
                    csvDataContextDriverProperties.DetectRelations &&
                    csvDataContextDriverProperties.UseSingleClassForSameFiles &&
                    csvDataContextDriverProperties.AllowComments == allowComments &&
                    csvDataContextDriverProperties.StringComparison == stringComparison &&
                    csvDataContextDriverProperties.IsStringInternEnabled &&
                    csvDataContextDriverProperties.IgnoreInvalidFiles &&
                    csvDataContextDriverProperties.IsCacheEnabled &&
                    csvDataContextDriverProperties.HideRelationsFromDump &&
                    csvDataContextDriverProperties.Persist);
        }
    }
}
