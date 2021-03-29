using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private static readonly string Files = Context.GetCsvFullPath("*.csv");
        private static readonly string[] ParsedFiles = { Files };

        [OneTimeSetUp]
        public void Init() =>
            Driver.Install("CsvLINQPadDriver", "CsvHelper.dll", "CsvLINQPadDriver.dll", "Humanizer.dll");

        [Test]
        [TestCaseSource(nameof(TestsData))]
        public void Execute_ScriptWithDriverProperties_Success((string linqScriptName, string? context, ICsvDataContextDriverProperties driverProperties) testsData)
        {
            var (linqScriptName, context, driverProperties) = testsData;

            var queryConfig = GetQueryHeaders().Aggregate(new StringBuilder(), (stringBuilder, h) =>
            {
                stringBuilder.AppendLine(h);
                stringBuilder.AppendLine();
                return stringBuilder;
            }).ToString();

            var linqScript = LinqScript.Create($"{linqScriptName}.linq", queryConfig);
            var (output, error, exitCode) = Runner.Execute(linqScript, TimeSpan.FromMinutes(2));

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine(output);
            }

            error.Should().BeNullOrWhiteSpace();
            exitCode.Should().Be(0);

            IEnumerable<string> GetQueryHeaders()
            {
                yield return ConnectionHeader.Get("CsvLINQPadDriver", "CsvLINQPadDriver.CsvDataContextDriver", driverProperties!);
                yield return @"string Reason([System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) => $""something went wrong at line #{sourceLineNumber}"";";
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
                .Select(stringComparison =>
                    ("StringComparison", 
                     $"new {{ {nameof(defaultCsvDataContextDriverProperties.StringComparison)} = {nameof(StringComparison)}.{stringComparison} }}",
                     GetDefaultCsvDataContextDriverPropertiesObject(stringComparison)));

            return multipleDriverPropertiesTestData
                    .Concat(singleDriverPropertiesTestData!)
                    .Concat(stringComparisonDriverPropertiesTestData)!;

            IEnumerable<ICsvDataContextDriverProperties> GetCsvDataContextDriverProperties()
            {
                yield return defaultCsvDataContextDriverProperties;

                yield return GetCsvDataContextDriverPropertiesObject(csvDataContextDriverProperties =>
                    csvDataContextDriverProperties.Files == Files &&
                    csvDataContextDriverProperties.ParsedFiles == ParsedFiles &&
                    csvDataContextDriverProperties.CsvSeparator == "," &&
                    csvDataContextDriverProperties.CsvSeparatorChar == ',' &&
                    csvDataContextDriverProperties.DebugInfo &&
                    csvDataContextDriverProperties.DetectRelations &&
                    csvDataContextDriverProperties.UseSingleClassForSameFiles == false &&
                    csvDataContextDriverProperties.StringComparison == defaultStringComparison &&
                    csvDataContextDriverProperties.IsStringInternEnabled == false &&
                    csvDataContextDriverProperties.IgnoreInvalidFiles == false &&
                    csvDataContextDriverProperties.IsCacheEnabled == false &&
                    csvDataContextDriverProperties.HideRelationsFromDump == false &&
                    csvDataContextDriverProperties.Persist == false
                );
            }

            static ICsvDataContextDriverProperties GetDefaultCsvDataContextDriverPropertiesObject(StringComparison stringComparison) =>
                GetCsvDataContextDriverPropertiesObject(csvDataContextDriverProperties =>
                    csvDataContextDriverProperties.Files == Files &&
                    csvDataContextDriverProperties.ParsedFiles == ParsedFiles &&
                    csvDataContextDriverProperties.CsvSeparator == "," &&
                    csvDataContextDriverProperties.CsvSeparatorChar == ',' &&
                    csvDataContextDriverProperties.DebugInfo &&
                    csvDataContextDriverProperties.DetectRelations &&
                    csvDataContextDriverProperties.UseSingleClassForSameFiles &&
                    csvDataContextDriverProperties.StringComparison == stringComparison &&
                    csvDataContextDriverProperties.IsStringInternEnabled &&
                    csvDataContextDriverProperties.IgnoreInvalidFiles &&
                    csvDataContextDriverProperties.IsCacheEnabled &&
                    csvDataContextDriverProperties.HideRelationsFromDump &&
                    csvDataContextDriverProperties.Persist
                );

            static ICsvDataContextDriverProperties GetCsvDataContextDriverPropertiesObject(
                Expression<Func<ICsvDataContextDriverProperties, bool>> predicate) =>
                Mock.Of(predicate, MockBehavior.Strict);
        }
    }
}
