using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public sealed class LPRunTests
    {
        private static readonly string Files = Context.GetDataFullPath("**.csv");

        private sealed record FileEncoding(string FileName, Encoding Encoding);

        [OneTimeSetUp]
        public void Init()
        {
            const string driverFileName = "CsvLINQPadDriver";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // TODO: Release with next LINQPad. Update readme.
            //Driver.EnsureNotInstalledViaNuGet(driverFileName);
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

        public sealed record ScriptWithDriverPropertiesTestData(string LinqScriptName, string? Context, ICsvDataContextDriverProperties DriverProperties, params string?[] Defines)
        {
            public int Index { get; init; }
        }

        [Test]
        [Parallelizable(ParallelScope.Children)]
        [TestCaseSource(nameof(ParallelizableScriptWithDriverPropertiesTestDataTestsData))]
        public async Task Execute_ScriptWithDriverProperties_Success(ScriptWithDriverPropertiesTestData testData)
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

            var linqScript = LinqScript.Create($"{linqScriptName}.linq", queryConfig, $"{linqScriptName}_{testData.Index}");

            Console.Write($"{linqScript}{Environment.NewLine}{Environment.NewLine}{queryConfig}");

            var (output, error, exitCode) = await Runner.ExecuteAsync(
                linqScript,
                TimeSpan.FromMinutes(2)
#if GITHUB_ACTIONS
                , new (3)
#endif
            );

            if (ShouldRender(output))
            {
                Console.WriteLine(output);
            }

            error.Should().BeNullOrWhiteSpace();
            exitCode.Should().Be(0);

            IEnumerable<string> GetQueryHeaders()
            {
                yield return ConnectionHeader.Get("CsvLINQPadDriver", "CsvLINQPadDriver.CsvDataContextDriver", driverProperties, "System.Runtime.CompilerServices", nameof(CsvLINQPadDriver));
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

        private static IEnumerable<ScriptWithDriverPropertiesTestData> ParallelizableScriptWithDriverPropertiesTestDataTestsData() =>
            ScriptWithDriverPropertiesTestDataTestsData().AugmentWithFileIndex(
                static testData => testData.LinqScriptName,
                static (testData, index) => testData with { Index = index });

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
                yield return new[] { "Extensions", "ExtensionsSafe", "SimilarFilesRelations" }
                    .Select(linqFile => new ScriptWithDriverPropertiesTestData(
                            linqFile,
                            noContext,
                            defaultCsvDataContextDriverProperties
#if NET5_0_OR_GREATER
                            , "NET5_0_OR_GREATER"
#endif
#if NET6_0_OR_GREATER
                            , "NET6_0_OR_GREATER"
#endif
#if NET7_0_OR_GREATER
                            , "NET7_0_OR_GREATER"
#endif
#if NET8_0_OR_GREATER
                            , "NET8_0_OR_GREATER"
#endif
                           ));

                // String comparison.
                yield return GetStringComparisons()
                    .SelectMany(static stringComparison => new[] { "StringComparison" }
                        .Select(linqFile => new ScriptWithDriverPropertiesTestData(
                            linqFile,
                            GetStringComparisonContext(stringComparison),
                            GetDefaultCsvDataContextDriverPropertiesObject(stringComparison))));

                // Encoding detection.
                yield return GetFileEncodings()
                    .SelectMany(static fileEncodings => new[] { "Encoding" }
                        .Select(linqFile => new ScriptWithDriverPropertiesTestData(
                            linqFile,
                            GetEncodingContext(fileEncodings),
                            GetDefaultCsvDataContextDriverPropertiesObject(defaultStringComparison))));

                // String comparison for interning.
                yield return GetStringComparisons()
                    .Select(static stringComparison => new ScriptWithDriverPropertiesTestData(
                            "StringComparisonForInterning",
                            GetStringComparisonContext(stringComparison),
                            GetDefaultCsvDataContextDriverPropertiesObject(
                                stringComparison,
                                useStringComparerForStringIntern: true)));

                // Allow comments.
                yield return new[] { true, false }
                    .Select(static allowComments => new ScriptWithDriverPropertiesTestData(
                            "Comments",
                            $"new {{ ExpectedCount = {(allowComments ? 1 : 2)} }}",
                            GetDefaultCsvDataContextDriverPropertiesObject(
                                defaultStringComparison,
                                allowComments)));

                // Skip leading rows.
                yield return new (string LinqScriptName, int Count)[] { ("SkipLeadingRows", 3), ("SkipLeadingRowsAll", 100) }
                    .Select(static skip => new ScriptWithDriverPropertiesTestData(
                            skip.LinqScriptName,
                            noContext,
                            GetDefaultCsvDataContextDriverPropertiesObject(
                                defaultStringComparison,
                                skipLeadingRowsCount: skip.Count)));

                // Rename table.
                yield return new[] { true, false }
                    .Select(static useSingleClassForSameFiles => new ScriptWithDriverPropertiesTestData(
                        "RenameTable",
                        noContext,
                        GetDefaultCsvDataContextDriverPropertiesObject(
                            defaultStringComparison,
                            useSingleClassForSameFiles: useSingleClassForSameFiles,
                            renameTable: true)));
            }

            IEnumerable<ICsvDataContextDriverProperties> GetCsvDataContextDriverProperties()
            {
                yield return defaultCsvDataContextDriverProperties;

                yield return GetCsvDataContextDriverPropertiesWithUseRecordType(false);
                yield return GetCsvDataContextDriverPropertiesWithUseRecordType(true);

                static ICsvDataContextDriverProperties GetCsvDataContextDriverPropertiesWithUseRecordType(bool useRecordType) =>
                    Mock.Of<ICsvDataContextDriverProperties>(csvDataContextDriverProperties =>
                        csvDataContextDriverProperties.Files == Files &&
                        csvDataContextDriverProperties.AutoDetectEncoding &&
                        csvDataContextDriverProperties.DebugInfo &&
                        csvDataContextDriverProperties.DetectRelations &&
                        csvDataContextDriverProperties.UseRecordType == useRecordType &&
                        csvDataContextDriverProperties.UseSingleClassForSameFiles == false &&
                        csvDataContextDriverProperties.StringComparison == defaultStringComparison &&
                        csvDataContextDriverProperties.IsStringInternEnabled == false &&
                        csvDataContextDriverProperties.IgnoreInvalidFiles == false &&
                        csvDataContextDriverProperties.IsCacheEnabled == false &&
                        csvDataContextDriverProperties.HideRelationsFromDump == false &&
                        csvDataContextDriverProperties.Persist == false
                    );
            }

            static ICsvDataContextDriverProperties GetDefaultCsvDataContextDriverPropertiesObject(
                StringComparison stringComparison,
                bool allowComments = false,
                bool useStringComparerForStringIntern = false,
                int skipLeadingRowsCount = 0,
                bool renameTable = false,
                bool useSingleClassForSameFiles = true,
                TableNameFormat tableNameFormat = TableNameFormat.table_1) =>
                Mock.Of<ICsvDataContextDriverProperties>(csvDataContextDriverProperties =>
                    csvDataContextDriverProperties.Files == Files &&
                    csvDataContextDriverProperties.AutoDetectEncoding &&
                    csvDataContextDriverProperties.DebugInfo &&
                    csvDataContextDriverProperties.DetectRelations &&
                    csvDataContextDriverProperties.UseRecordType &&
                    csvDataContextDriverProperties.UseSingleClassForSameFiles == useSingleClassForSameFiles &&
                    csvDataContextDriverProperties.AllowComments == allowComments &&
                    csvDataContextDriverProperties.StringComparison == stringComparison &&
                    csvDataContextDriverProperties.IsStringInternEnabled &&
                    csvDataContextDriverProperties.UseStringComparerForStringIntern == useStringComparerForStringIntern &&
                    csvDataContextDriverProperties.IgnoreInvalidFiles &&
                    csvDataContextDriverProperties.IsCacheEnabled &&
                    csvDataContextDriverProperties.HideRelationsFromDump &&
                    csvDataContextDriverProperties.Persist &&
                    csvDataContextDriverProperties.AllowSkipLeadingRows &&
                    csvDataContextDriverProperties.SkipLeadingRowsCount == skipLeadingRowsCount &&
                    csvDataContextDriverProperties.RenameTable == renameTable &&
                    csvDataContextDriverProperties.TableNameFormat == tableNameFormat
                );

            static IEnumerable<StringComparison> GetStringComparisons() =>
                Enum.GetValues(typeof(StringComparison)).Cast<StringComparison>();

            static string GetStringComparisonContext(StringComparison stringComparison) =>
                $"new {{ StringComparison = StringComparison.{stringComparison} }}";

            static string GetEncodingContext(IEnumerable<string> objects)
            {
                return $@"new[]
{{
{string.Join($",{Environment.NewLine}", objects.Select(static obj => $"\t{obj}"))}
}}";
            }

            static IEnumerable<IEnumerable<string>> GetFileEncodings()
            {
                yield return GetBomFiles();
                yield return GetEncodingDetectionFiles();

                static IEnumerable<string> GetBomFiles()
                {
                    yield return "Utf8Cp65001_Encoding";
                    yield return "Utf16BomCp1200_Encoding";
                    yield return "Utf16BomCp1201_Encoding";
                    yield return "Utf8BomCp65001_Encoding";
                    yield return "Utf32Bom_Encoding";
                }

                static IEnumerable<string> GetEncodingDetectionFiles()
                {
                    yield return "Cp1252_Encoding_German";
                    yield return "Utf16BomCp1200_Encoding_German";
                    yield return "Utf16BomCp1201_Encoding_German";
                    yield return "Utf8BomCp65001_Encoding_German";
                    yield return "Utf32Bom_Encoding_German";
                }
            }
        }
    }
}
