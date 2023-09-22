using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using LPRun;

namespace CsvLINQPadDriverTest
{
    [TestFixture]
    public sealed partial class LPRunTests
    {
        private const           int SuccessExitCode = 0;
        private static readonly int ErrorExitCode   = new Random().Next() % 250 + 2;

        private static readonly string SuccessMessage = Guid.NewGuid().ToString();
        private static readonly string ErrorMessage   = Guid.NewGuid().ToString();

        public sealed record ScriptFromSourceTestData(bool Succeeded, string Payload, bool HasErrorOutput = false, int? ExitCode = null);

        [Test]
        [Parallelizable(ParallelScope.Children)]
        [TestCaseSource(nameof(ScriptFromSourceTestDataTestsData))]
        public async Task Execute_ScriptFromSource_Success(ScriptFromSourceTestData testData)
        {
            var scriptFile = LinqScript.FromScript(GetScript(), @"<Query Kind=""Program"" />");

            var result = await ExecuteAsync(scriptFile);

            result.Success.Should().Be(testData.Succeeded);
            result.ExitCode.Should().Be(testData.ExitCode ?? (testData.Succeeded ? SuccessExitCode : ErrorExitCode));

            if (testData.HasErrorOutput)
            {
                result.Error.Should().NotBeNullOrEmpty().And.Contain(ErrorMessage);
            }
            else
            {
                result.Error.Should().BeNullOrEmpty();
            }

            string GetScript() => $$"""
                                    int Main()
                                    {
                                        {{testData.Payload}};
                                        return {{SuccessExitCode}};
                                    }
                                    """;
        }

        private static IEnumerable<ScriptFromSourceTestData> ScriptFromSourceTestDataTestsData()
        {
            yield return new(false, $@"Console.Error.WriteLine(""{ErrorMessage}"")", true, SuccessExitCode);
            yield return new(true,  $@"Console.WriteLine(""{SuccessMessage}"")");
            yield return new(true,  $"Environment.Exit({SuccessExitCode})");
            yield return new(false, $"Environment.Exit({ErrorExitCode})");
            yield return new(true,  "//");
            yield return new(false, $@"throw new Exception(""{ErrorMessage}"")", true, 1); // LPRun exit code.
            yield return new(false, $"return {ErrorExitCode}");
            yield return new(true,  $"return {SuccessExitCode}");
        }
    }
}
