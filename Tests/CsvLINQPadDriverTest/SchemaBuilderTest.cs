using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LINQPad.Extensibility.DataContext;
using CsvLINQPadDriver;
using NUnit.Framework;

namespace CsvLINQPadDriverTest
{
    [TestFixture]
    public class SchemaBuilderTest
    {
        [Test]
        public void GetSchemaAndBuildAssemblyTest1()
        {
            GetSchemaAndBuildAssemblyTest(new PropertiesMock()
            {
                Files = Path.Combine(Directory.GetCurrentDirectory(), "*.csv"),
                DebugInfo = true,
                DetectRelations = true,
                IgnoreInvalidFiles = true,
                IsCacheEnabled = true,
                HideRelationsFromDump = true,
                Persist = true,
            }, "1");
        }

        [Test]
        public void GetSchemaAndBuildAssemblyTest2()
        {
            GetSchemaAndBuildAssemblyTest(new PropertiesMock()
            {
                Files = Path.Combine(Directory.GetCurrentDirectory(), "*.csv"),
                DebugInfo = true,
                DetectRelations = true,
                IgnoreInvalidFiles = false,
                IsCacheEnabled = false,
                HideRelationsFromDump = false,
                Persist = false,
            }, "2");
        }

        public void GetSchemaAndBuildAssemblyTest(ICsvDataContextDriverProperties properties, string id)
        {
            File.WriteAllText("TestA.csv",
@"a,b,c,TestAID
1,2,3,1
1,2,3,2
x");
            File.WriteAllText("TestB.csv",
@"c,d,e,TestAID
1,2,3,1
1,2,3,2
x");

            string nameSpace = "TestContextNamespace";
            string contextTypeName = "TestContextClass";
            var contextAssemblyName = new AssemblyName("TestContextAssembly" + id)
            {   
                CodeBase = "TestContextAssembly" + id + ".dll"
            };

            if (File.Exists(contextAssemblyName.CodeBase))
                File.Delete(contextAssemblyName.CodeBase);

            var explorerItems = SchemaBuilder.GetSchemaAndBuildAssembly(
                properties,
                contextAssemblyName,
                ref nameSpace,
                ref contextTypeName
            );

            //debug info to console
            Console.WriteLine(explorerItems[0].DragText);
            Console.WriteLine(explorerItems[1].DragText);

            //check returned explorer tree
            Assert.AreEqual( 3, explorerItems.Count, "explorer items count");
            explorerItems = explorerItems.Where(i => i.Kind == ExplorerItemKind.QueryableObject).ToList();
            Assert.AreEqual( "TestA,TestB", string.Join(",", explorerItems.Select(i => i.DragText)));
            Assert.AreEqual( "a,b,c,TestAID,TestB,c,d,e,TestAID,TestA", string.Join(",", explorerItems.SelectMany(i => i.Children.Select(c => c.DragText))));

            //check compiled assembly
            var contextAssembly = Assembly.Load(contextAssemblyName);
            Assert.AreEqual("CsvDataContext,TTestA,TTestB", string.Join(",", contextAssembly.GetExportedTypes().Select(t => t.Name)));

            var contextType = contextAssembly.GetType(nameSpace + "." + contextTypeName);
            Assert.IsNotNull(contextType, "ContextType in assembly");

            //check generated context runtime
            dynamic contextInstance = contextType.GetConstructor(new Type[] {}).Invoke(new object[] {});
            Assert.IsNotNull(contextInstance, "context created");

            dynamic dataFirst = Enumerable.ToArray(contextInstance.TestA)[0];
            Assert.AreEqual("3", dataFirst.c);
            Assert.AreEqual(1, Enumerable.Count(dataFirst.TestB));
        }

        class PropertiesMock : ICsvDataContextDriverProperties
        {
            public bool Persist { get; set; }
            public string Files { get; set; }
            public string CsvSeparator { get; set; }
            public char? CsvSeparatorChar { get; internal set; }
            public bool DetectRelations { get; set; }
            public bool HideRelationsFromDump { get; set; }
            public bool DebugInfo { get; set; }
            public bool IgnoreInvalidFiles { get; set; }
            public bool IsStringInternEnabled { get; set; }
            public bool IsCacheEnabled { get; set; }
        }
    }
}
