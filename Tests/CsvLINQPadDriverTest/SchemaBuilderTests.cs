using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LINQPad.Extensibility.DataContext;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using CsvLINQPadDriver;

namespace CsvLINQPadDriverTest
{
    [TestFixture]
    public class SchemaBuilderTests
    {
        [Test]
        [TestCaseSource(nameof(CsvDataContextDriverProperties))]
        public void GetSchemaAndBuildAssembly_CreatedAssembly_AsExpected((ICsvDataContextDriverProperties, int) testData)
        {
            var (properties, id) = testData;

            File.WriteAllText("TestA.csv",
@"a,b,c,TestAID
1,2,3,11
1,2,3,12
x");
            File.WriteAllText("TestB.csv",
@"c,d,e,TestAID
1,2,3,11
1,2,3,12
x");

            var nameSpace = "TestContextNamespace";
            var contextTypeName = "TestContextClass";
            var contextAssemblyName = new AssemblyName($"TestContextAssembly{id}")
            {
                CodeBase = $"TestContextAssembly{id}.dll"
            };

            if (File.Exists(contextAssemblyName.CodeBase))
            {
                File.Delete(contextAssemblyName.CodeBase);
            }

            var explorerItems = SchemaBuilder.GetSchemaAndBuildAssembly(
                properties,
                contextAssemblyName,
                ref nameSpace,
                ref contextTypeName
            ).ToImmutableList();

            // Debug info to console.
            explorerItems.ForEach(item => Console.WriteLine(item.DragText));

            // Check returned explorer tree.
            explorerItems.Should().HaveCount(3);

            explorerItems = explorerItems.Where(i => i.Kind == ExplorerItemKind.QueryableObject).ToImmutableList();

            explorerItems.Select(i => i.DragText)
                .Should()
                .BeEquivalentTo(Split("TestA,TestB"));

            explorerItems.SelectMany(i => i.Children.Select(c => c.DragText))
                .Should()
                .BeEquivalentTo(Split("a,b,c,TestAID,TestB,c,d,e,TestAID,TestA"));

            // Check compiled assembly.
            var contextAssembly = Assembly.Load(contextAssemblyName);
            contextAssembly.GetExportedTypes().Select(type => type.Name)
                .Should()
                .BeEquivalentTo(Split("CsvDataContext,TTestA,TTestB"));

            var contextType = contextAssembly.GetType($"{nameSpace}.{contextTypeName}");
            contextType.Should().NotBeNull("ContextType in assembly");

            // Check generated context runtime.
            var contextInstance = contextType!.GetConstructor(new Type[] {})
                .Should()
                .NotBeNull()
                .And
                .Subject.Invoke(new object[] {});

            contextInstance.Should().NotBeNull("context created");

            dynamic dataFirst = Enumerable.ToArray(((dynamic)contextInstance).TestA)[0];

            ((string)dataFirst.c).Should().Be("3");
            ((IEnumerable)dataFirst.TestB).Should().HaveCount(1);

            static IEnumerable<string> Split(string str) =>
                str.Split(",");
        }

        private static IEnumerable<(ICsvDataContextDriverProperties, int)> CsvDataContextDriverProperties()
        {
            var files = Path.Combine(Directory.GetCurrentDirectory(), "*.csv");
            var parsedFiles = new[] { files };

            yield return (GetCsvDataContextDriverProperties(csvDataContextDriverProperties =>
                csvDataContextDriverProperties.Files == files &&
                csvDataContextDriverProperties.ParsedFiles == parsedFiles &&
                csvDataContextDriverProperties.CsvSeparatorChar == ',' &&
                csvDataContextDriverProperties.DebugInfo &&
                csvDataContextDriverProperties.DetectRelations &&
                csvDataContextDriverProperties.UseSingleClassForSameFiles &&
                csvDataContextDriverProperties.StringComparison == StringComparison.InvariantCulture &&
                csvDataContextDriverProperties.IsStringInternEnabled &&
                csvDataContextDriverProperties.IgnoreInvalidFiles &&
                csvDataContextDriverProperties.IsCacheEnabled &&
                csvDataContextDriverProperties.HideRelationsFromDump &&
                csvDataContextDriverProperties.Persist
            ), 1);

            yield return (GetCsvDataContextDriverProperties(csvDataContextDriverProperties =>
                csvDataContextDriverProperties.Files == files &&
                csvDataContextDriverProperties.ParsedFiles == parsedFiles &&
                csvDataContextDriverProperties.CsvSeparatorChar == ',' &&
                csvDataContextDriverProperties.DebugInfo &&
                csvDataContextDriverProperties.DetectRelations &&
                csvDataContextDriverProperties.UseSingleClassForSameFiles == false &&
                csvDataContextDriverProperties.StringComparison == StringComparison.InvariantCulture &&
                csvDataContextDriverProperties.IsStringInternEnabled == false &&
                csvDataContextDriverProperties.IgnoreInvalidFiles == false &&
                csvDataContextDriverProperties.IsCacheEnabled == false &&
                csvDataContextDriverProperties.HideRelationsFromDump == false &&
                csvDataContextDriverProperties.Persist == false
            ), 2);

            static ICsvDataContextDriverProperties GetCsvDataContextDriverProperties(Expression<Func<ICsvDataContextDriverProperties, bool>> predicate) =>
                Mock.Of(predicate, MockBehavior.Strict);
        }
    }
}
