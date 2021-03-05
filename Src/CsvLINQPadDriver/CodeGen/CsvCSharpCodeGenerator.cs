using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Security;

using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.DataModel;
using CsvLINQPadDriver.Helpers;

using Microsoft.CSharp;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvCSharpCodeGenerator
    {
        private const string DefaultContextTypeName = "CsvDataContext";

        private readonly ICsvDataContextDriverProperties _properties;

        private readonly string _contextNameSpace;
        private readonly string _contextTypeName;

        private CsvCSharpCodeGenerator(string contextNameSpace, string contextTypeName, ICsvDataContextDriverProperties properties)
        {
            _contextNameSpace = contextNameSpace;
            _contextTypeName = contextTypeName;
            _properties = properties;
        }

        // ReSharper disable once RedundantAssignment
        public static string GenerateCode(CsvDatabase db, ref string nameSpace, ref string typeName, ICsvDataContextDriverProperties props) =>
            new CsvCSharpCodeGenerator(nameSpace, typeName = DefaultContextTypeName, props).GenerateSrcFile(db);

        private string GenerateSrcFile(CsvDatabase csvDatabase) =>
                $@"using System;
using System.Linq;
using System.Collections.Generic;

namespace {_contextNameSpace}
{{
    /// <summary>CSV Data Context</summary>
    public class {_contextTypeName} : {typeof(CsvDataContextBase).GetCodeTypeClassName()}
    {{ {string.Join(string.Empty, csvDatabase.Tables.Select(table => $@"
        /// <summary>File: {SecurityElement.Escape(table.FilePath)}</summary>
        public {typeof(CsvTableBase<>).GetCodeTypeClassName(table.GetCodeRowClassName())} {table.CodeName} {{ get; private set; }}")
                )}

        public {_contextTypeName}()
        {{
            //Init tables data {string.Join(string.Empty, csvDatabase.Tables.Select(table => $@"
            this.{table.CodeName} = {typeof(CsvTableFactory).GetCodeTypeClassName()}.CreateTable<{table.GetCodeRowClassName()}>(
                {(_properties.IsStringInternEnabled ? "true" : "false")},
                {(_properties.IsCacheEnabled ? "true" : "false")},
                {table.CsvSeparator.AsValidCSharpCode()},
                {table.FilePath.AsValidCSharpCode()},
                new {typeof(CsvColumnInfoList<>).GetCodeTypeClassName(table.GetCodeRowClassName())}() {{
                    {string.Join(string.Empty, table.Columns.Select(c => $@"{{ {c.CsvColumnIndex}, x => x.{c.CodeName} }}, "))}
                }},
                r => {{ {string.Join(string.Empty, table.Relations.Select(csvRelation => $@"
                    r.{csvRelation.CodeName} = new {typeof(LazyEnumerable<>).GetCodeTypeClassName(csvRelation.TargetTable.GetCodeRowClassName())}(
                        () => {csvRelation.TargetTable.CodeName}.WhereIndexed(tr => tr.{csvRelation.TargetColumn.CodeName}, {csvRelation.TargetColumn.CodeName.AsValidCSharpCode()}, r.{csvRelation.SourceColumn.CodeName}));"))}
                }}
            );")
                )}
        }}
    }}//context class

    //Data types {string.Join(string.Empty, csvDatabase.Tables.Select(table => GenerateTableRowDataTypeClass(table, _properties.HideRelationsFromDump)))}
}}//namespace
";

        private static string GenerateTableRowDataTypeClass(CsvTable table, bool hideRelationsFromDump) =>
            $@"
    public class {table.GetCodeRowClassName()} : {typeof(ICsvRowBase).GetCodeTypeClassName()}
    {{{string.Join(string.Empty, table.Columns.Select(c => $@"
        public string {c.CodeName} {{ get; set; }} ")
            )}{string.Join(string.Empty, table.Relations.Select(csvRelation => $@"
        /// <summary>{SecurityElement.Escape(csvRelation.DisplayName)}</summary> {(hideRelationsFromDump ? $@"
        [{typeof(HideFromDumpAttribute).GetCodeTypeClassName()}]" : string.Empty)}
        public IEnumerable<{csvRelation.TargetTable.GetCodeRowClassName()}> {csvRelation.CodeName} {{ get; set; }} ")
            )}
    }} ";
    }

    internal static class CsvCSharpCodeGeneratorExtensions
    {
        public static string GetCodeRowClassName(this CsvTable table) =>
            $"T{table.CodeName}";

        public static string GetCodeTypeClassName(this Type type, params string[] genericParameters) =>
            type!.FullName!.Split('`')[0] + (genericParameters.Any() ? $"<{string.Join(",", genericParameters)}>" : string.Empty);

        public static string AsValidCSharpCode<T>(this T input)
        {
            using var stringWriter = new StringWriter();
            using var csharpCodeProvider = new CSharpCodeProvider();

            csharpCodeProvider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), stringWriter, null);

            return stringWriter.ToString();
        }
    }
}
