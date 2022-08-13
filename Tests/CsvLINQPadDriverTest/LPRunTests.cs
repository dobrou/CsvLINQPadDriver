using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Moq;

using NUnit.Framework;

using CsvLINQPadDriver;
using CsvLINQPadDriver.Extensions;

using LPRun;

#pragma warning disable CA1416

namespace CsvLINQPadDriverTest
{
    [TestFixture]
    public class LPRunTests
    {
        private static readonly string Files = Context.GetDataFullPath("**.csv");

        [OneTimeSetUp]
        public void Init()
        {
            const string driverFileName = "CsvLINQPadDriver";

            Driver.InstallWithDepsJson(driverFileName, $"{driverFileName}.dll", "Tests");

            CreateEncodingFiles();

            static void CreateEncodingFiles()
            {
                var contents = File.ReadAllText(GetFilePath("Utf8Cp65001"), Encoding.Default);

                Array.ForEach(GetEncodings().ToArray(), WriteFiles);

                void WriteFiles((string FileName, Encoding Encoding) fileEncoding) =>
                    File.WriteAllText(GetFilePath(fileEncoding.FileName), contents, fileEncoding.Encoding);

                static IEnumerable<(string FileName, Encoding Encoding)> GetEncodings()
                {
                    yield return ("Utf16BomCp1200", Encoding.Unicode);
                    yield return ("Utf16BomCp1201", Encoding.BigEndianUnicode);
                    yield return ("Utf8BomCp65001", Encoding.UTF8);
                    yield return ("Utf32Bom",       Encoding.UTF32);
                }

                static string GetFilePath(string fileName) =>
                    Context.GetDataFullPath(Path.Combine("Encoding", $"{fileName}.csv"));
            }
        }

        public record ScriptWithDriverPropertiesTestData(string LinqScriptName, string? Context, ICsvDataContextDriverProperties DriverProperties, params string?[] Defines);

        [Test]
        [TestCaseSource(nameof(ScriptWithDriverPropertiesTestDataTestsData))]
        public void Execute_ScriptWithDriverProperties_Success(ScriptWithDriverPropertiesTestData testData)
        {
            var (linqScriptName, context, driverProperties, defines) = testData;

            var queryConfig = GetQueryHeaders().Aggregate(new StringBuilder(), static (stringBuilder, header) =>
            {
                if (ShouldRender(header))
                {
                    stringBuilder.AppendLine(header);
                    stringBuilder.AppendLine();
                }

                return stringBuilder;
            }).ToString();

            var linqScript = LinqScript.Create($"{linqScriptName}.linq", queryConfig);

            Console.Write($"{linqScript}{Environment.NewLine}{Environment.NewLine}{queryConfig}");

            var (output, error, exitCode) = Runner.Execute(linqScript);

            if (ShouldRender(output))
            {
                Console.WriteLine(output);
            }

            error.Should().BeNullOrWhiteSpace();
            exitCode.Should().Be(0);

            IEnumerable<string> GetQueryHeaders()
            {
                yield return ConnectionHeader.Get("CsvLINQPadDriver", "CsvLINQPadDriver.CsvDataContextDriver", driverProperties, "System.Runtime.CompilerServices");
                yield return defines.Where(ShouldRender).Select(static define => $"#define {define}").JoinNewLine();
                yield return @"string Reason([CallerLineNumber] int sourceLineNumber = 0) => $""something went wrong at line #{sourceLineNumber}"";";
                if (ShouldRender(context))
                {
                    yield return $"var context = {context};";
                }
            }

            static bool ShouldRender(string? str) =>
                !string.IsNullOrWhiteSpace(str);
        }

        private static IEnumerable<ScriptWithDriverPropertiesTestData> ScriptWithDriverPropertiesTestDataTestsData()
        {
            const StringComparison defaultStringComparison = StringComparison.InvariantCulture;

            var defaultCsvDataContextDriverProperties = GetDefaultCsvDataContextDriverPropertiesObject(defaultStringComparison);

            return GetTestData().SelectMany(_ => _);

            IEnumerable<IEnumerable<ScriptWithDriverPropertiesTestData>> GetTestData()
            {
                const string? noContext = null;

                // Multiple driver properties.
                yield return GetCsvDataContextDriverProperties()
                    .SelectMany(static driverProperties => new[] { "Generation", "Relations"}
                        .Select(linqScriptName => new ScriptWithDriverPropertiesTestData(
                            linqScriptName,
                            $"new {{ {nameof(driverProperties.UseSingleClassForSameFiles)} = {driverProperties.UseSingleClassForSameFiles.ToString().ToLowerInvariant()} }}",
                            driverProperties,
                            driverProperties.UseRecordType ? "USE_RECORD_TYPE" : null)));

                // Single driver properties.
                yield return new[] { "Extensions", "SimilarFilesRelations" }
                    .Select(linqFile => new ScriptWithDriverPropertiesTestData(
                            linqFile,
                            noContext,
                            defaultCsvDataContextDriverProperties
#if NET6_0_OR_GREATER
                            , "NET6_0_OR_GREATER"
#endif
                           ));

                // String comparison.
                yield return GetStringComparisons()
                    .SelectMany(static stringComparison => new[] { "StringComparison", "Encoding" }
                        .Select(linqFile => new ScriptWithDriverPropertiesTestData(
                            linqFile,
                            GetStringComparisonContext(stringComparison),
                            GetDefaultCsvDataContextDriverPropertiesObject(stringComparison))));

                // String comparison for interning.
                yield return GetStringComparisons()
                    .Select(static stringComparison => new ScriptWithDriverPropertiesTestData(
                            "StringComparisonForInterning",
                            GetStringComparisonContext(stringComparison),
                            GetDefaultCsvDataContextDriverPropertiesObject(stringComparison, useStringComparerForStringIntern: true)));

                // Allow comments.
                yield return new[] { true, false }
                    .Select(static allowComments => new ScriptWithDriverPropertiesTestData(
                            "Comments",
                            $"new {{ ExpectedCount = {(allowComments ? 1 : 2)} }}",
                            GetDefaultCsvDataContextDriverPropertiesObject(defaultStringComparison, allowComments)));

                // Skip leading rows.
                yield return new (string LinqScriptName, int Count)[] { ("SkipLeadingRows", 3), ("SkipLeadingRowsAll", 100) }
                    .Select(static skip => new ScriptWithDriverPropertiesTestData(
                            skip.LinqScriptName,
                            noContext,
                            GetDefaultCsvDataContextDriverPropertiesObject(defaultStringComparison, skipLeadingRowsCount: skip.Count)));
            }

            IEnumerable<ICsvDataContextDriverProperties> GetCsvDataContextDriverProperties()
            {
                yield return defaultCsvDataContextDriverProperties;

                yield return GetCsvDataContextDriverPropertiesWithUseRecordType(false);
                yield return GetCsvDataContextDriverPropertiesWithUseRecordType(true);

                static ICsvDataContextDriverProperties GetCsvDataContextDriverPropertiesWithUseRecordType(bool useRecordType) =>
                    Mock.Of<ICsvDataContextDriverProperties>(csvDataContextDriverProperties =>
                        csvDataContextDriverProperties.Files == Files &&
                        csvDataContextDriverProperties.DebugInfo &&
                        csvDataContextDriverProperties.DetectRelations &&
                        csvDataContextDriverProperties.UseRecordType == useRecordType &&
                        csvDataContextDriverProperties.UseSingleClassForSameFiles == false &&
                        csvDataContextDriverProperties.StringComparison == defaultStringComparison &&
                        csvDataContextDriverProperties.IsStringInternEnabled == false &&
                        csvDataContextDriverProperties.IgnoreInvalidFiles == false &&
                        csvDataContextDriverProperties.IsCacheEnabled == false &&
                        csvDataContextDriverProperties.HideRelationsFromDump == false &&
                        csvDataContextDriverProperties.Persist == false);
            }

            static ICsvDataContextDriverProperties GetDefaultCsvDataContextDriverPropertiesObject(
                StringComparison stringComparison,
                bool allowComments = false,
                bool useStringComparerForStringIntern = false,
                int skipLeadingRowsCount = 0) =>
                Mock.Of<ICsvDataContextDriverProperties>(csvDataContextDriverProperties =>
                    csvDataContextDriverProperties.Files == Files &&
                    csvDataContextDriverProperties.DebugInfo &&
                    csvDataContextDriverProperties.DetectRelations &&
                    csvDataContextDriverProperties.UseRecordType &&
                    csvDataContextDriverProperties.UseSingleClassForSameFiles &&
                    csvDataContextDriverProperties.AllowComments == allowComments &&
                    csvDataContextDriverProperties.StringComparison == stringComparison &&
                    csvDataContextDriverProperties.IsStringInternEnabled &&
                    csvDataContextDriverProperties.UseStringComparerForStringIntern == useStringComparerForStringIntern &&
                    csvDataContextDriverProperties.IgnoreInvalidFiles &&
                    csvDataContextDriverProperties.IsCacheEnabled &&
                    csvDataContextDriverProperties.HideRelationsFromDump &&
                    csvDataContextDriverProperties.Persist &&
                    csvDataContextDriverProperties.AllowSkipLeadingRows &&
                    csvDataContextDriverProperties.SkipLeadingRowsCount == skipLeadingRowsCount);

            static IEnumerable<StringComparison> GetStringComparisons() =>
                Enum.GetValues(typeof(StringComparison)).Cast<StringComparison>();

            static string GetStringComparisonContext(StringComparison stringComparison) =>
                $"new {{ StringComparison = StringComparison.{stringComparison} }}";
        }
    }
}
