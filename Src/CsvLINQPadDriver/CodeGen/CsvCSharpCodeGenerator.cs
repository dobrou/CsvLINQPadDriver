using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;

using Microsoft.CSharp;

using Humanizer;

using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.DataModel;
using CsvLINQPadDriver.Extensions;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvCSharpCodeGenerator
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

        public record TypeCodeResult(string TypeName, string Code, string CodeName, string FilePath);
        public record Result(string Code, IReadOnlyCollection<IGrouping<string, TypeCodeResult>> CodeGroups);

        // ReSharper disable once RedundantAssignment
        internal static Result GenerateCode(CsvDatabase db, ref string nameSpace, ref string typeName, ICsvDataContextDriverProperties props) =>
            new CsvCSharpCodeGenerator(nameSpace, typeName = DefaultContextTypeName, props).GenerateSrcFile(db);

        private Result GenerateSrcFile(CsvDatabase csvDatabase)
        {
            var csvTables = csvDatabase.Tables;

            var groups = csvTables
                    .Select(table => GenerateTableRowDataTypeClass(table, _properties.HideRelationsFromDump, _properties.StringComparison))
                    .GroupBy(typeCode => typeCode.TypeName)
                    .ToImmutableList();

            return new Result($@"using System;
using System.Collections.Generic;

using CsvLINQPadDriver;

namespace {_contextNameSpace}
{{
    /// <summary>CSV Data Context</summary>
    public class {_contextTypeName} : {typeof(CsvDataContextBase).GetCodeTypeClassName()}
    {{{string.Join(string.Empty, csvTables.Select(table => $@"
        /// <summary>File: {SecurityElement.Escape(table.FilePath)}</summary>
        public {typeof(CsvTableBase<>).GetCodeTypeClassName(GetClassName(table))} {table.CodeName} {{ get; private set; }}")
                )}

        public {_contextTypeName}()
        {{
            // Init tables data {string.Join(string.Empty, csvTables.Select(table => $@"
            this.{table.CodeName} = {typeof(CsvTableFactory).GetCodeTypeClassName()}.CreateTable<{GetClassName(table)}>(
                {GetBoolConst(_properties.IsStringInternEnabled)},
                {GetBoolConst(_properties.IsCacheEnabled)},
                {table.CsvSeparator.AsValidCSharpCode()},
                {nameof(NoBomEncoding)}.{_properties.NoBomEncoding},
                {GetBoolConst(_properties.AllowComments)},
                {table.FilePath.AsValidCSharpCode()},
                new {typeof(CsvColumnInfoList<>).GetCodeTypeClassName(GetClassName(table))} {{
                    {string.Join(string.Empty, table.Columns.Select(c => $@"{{ {c.Index}, x => x.{c.CodeName} }}, "))}
                }},
                r => {{{string.Join(string.Empty, table.Relations.Select(csvRelation => $@"
                    r.{csvRelation.CodeName} = new {typeof(LazyEnumerable<>).GetCodeTypeClassName(GetClassName(csvRelation.TargetTable))}(
                        () => {csvRelation.TargetTable.CodeName}.WhereIndexed(tr => tr.{csvRelation.TargetColumn.CodeName}, {csvRelation.TargetColumn.CodeName.AsValidCSharpCode()}, r.{csvRelation.SourceColumn.CodeName}));"))}
                }}
            );")
                )}
        }}
    }} // context class

    // Data types {string.Join(Environment.NewLine, groups.Select(grouping => grouping.First().Code))} // data types
}} // namespace
", groups);

            static string GetBoolConst(bool val) =>
                val ? "true" : "false";
        }

        private static TypeCodeResult GenerateTableRowDataTypeClass(CsvTable table, bool hideRelationsFromDump, StringComparison stringComparison)
        {
            var className = GetClassName(table);
            var properties = table.Columns.Select(GetPropertyName).ToImmutableList();

            return new TypeCodeResult(className, $@"
    public sealed record {className} : {typeof(ICsvRowBase).GetCodeTypeClassName()}
    {{{string.Join(string.Empty, table.Columns.Select(csvColumn => $@"
        public string {GetPropertyName(csvColumn)} {{ get; set; }}"))}
        {GenerateIndexer(properties, true)}
        {GenerateIndexer(properties, false)}
        {GenerateToString(properties)}
        {GenerateEqualsAndGetHashCode(className, properties, stringComparison)}{string.Join(string.Empty, table.Relations.Select(csvRelation => $@"

        /// <summary>{SecurityElement.Escape(csvRelation.DisplayName)}</summary> {(hideRelationsFromDump ? $@"
        [{typeof(HideFromDumpAttribute).GetCodeTypeClassName()}]" : string.Empty)}
        public IEnumerable<{csvRelation.TargetTable.GetCodeRowClassName()}> {csvRelation.CodeName} {{ get; set; }}")
            )}
    }}", table.CodeName!, table.FilePath);

            static string GetPropertyName(ICsvNames csvColumn) =>
                csvColumn.CodeName!;
        }

        private static string GenerateIndexer(IReadOnlyCollection<string> properties, bool intIndexer)
        {
            return $@"
        [{typeof(HideFromDumpAttribute).GetCodeTypeClassName()}]
        public string this[{(intIndexer ? "int" : "string")} index]
        {{
            get
            {{
                switch(index)
                {{{string.Join(string.Empty, properties.Select((c, i) => $@"
                    case {(intIndexer ? IntToStr(i) : $"\"{c}\"")}: return {c};"))}
                    {GenerateIndexerException(intIndexer)}
                }}
            }}
            set
            {{
                switch(index)
                {{{string.Join(string.Empty, properties.Select((c, i) => $@"
                    case {(intIndexer ? IntToStr(i) : $"\"{c}\"")}: {c} = value; return;"))}
                    {GenerateIndexerException(intIndexer)}
                }}
            }}
        }}";

            static string GenerateIndexerException(bool intIndexer) =>
                $@"default: throw new IndexOutOfRangeException(string.Format(""There is no property {(intIndexer ? "at index {0}" : "with name \\\"{0}\\\"")}"", index));";

            static string IntToStr(int val) =>
                val.ToString(CultureInfo.InvariantCulture);
        }

        private static string GenerateToString(IReadOnlyCollection<string> properties)
        {
            var namePadding = properties.Max(property => property.Length);

            return $@"
        public override string ToString()
        {{
            return string.Format(""{string.Join(string.Empty, properties.Select((v, i) => $"{v.PadRight(namePadding)} : {{{i+1}}}{{0}}"))}"", Environment.NewLine, {string.Join(", ", properties)});
        }}";
        }

        private static string GenerateEqualsAndGetHashCode(string typeName, IReadOnlyCollection<string> properties, StringComparison stringComparison) =>
            $@"
        public bool Equals({typeName} obj)
        {{
            if(obj == null) return false;
            if(ReferenceEquals(this, obj)) return true;
            return  {string.Join($" && {Environment.NewLine}{GetIndent(20)}",
                properties.Select(property => $"string.Equals({property}, obj.{property}, StringComparison.{stringComparison})"))};
        }}

        public override int GetHashCode()
        {{
            var hashCode = new HashCode();

{string.Join(Environment.NewLine, properties.Select(property => $"{GetIndent(12)}hashCode.Add({property}, StringComparer.{GetStringComparer(stringComparison)});"))}

            return hashCode.ToHashCode();
        }}";

        private static string GetClassName(CsvTable table) =>
            table.GetCodeRowClassName();

        private static string GetIndent(int count) =>
            new(' ', count);

        private static string GetStringComparer(StringComparison stringComparison) =>
            stringComparison switch
            {
                StringComparison.CurrentCulture => nameof(StringComparer.CurrentCulture), 
                StringComparison.CurrentCultureIgnoreCase => nameof(StringComparer.CurrentCultureIgnoreCase), 
                StringComparison.InvariantCulture => nameof(StringComparer.InvariantCulture), 
                StringComparison.InvariantCultureIgnoreCase => nameof(StringComparer.InvariantCultureIgnoreCase), 
                StringComparison.Ordinal => nameof(StringComparer.Ordinal), 
                StringComparison.OrdinalIgnoreCase => nameof(StringComparer.OrdinalIgnoreCase),
                _ => throw new ArgumentException($"Unknown string comparison {stringComparison}", nameof(stringComparison))
            };
    }

    internal static class CsvCSharpCodeGeneratorExtensions
    {
        public static string GetCodeRowClassName(this CsvTable table)
        {
            return ToClassName(table.ClassName);

            static string ToClassName(string? name) =>
                string.IsNullOrWhiteSpace(name)
                    ? throw new ArgumentNullException(nameof(name), "Name is null or empty")
                    : $"R{(name.Length < 3 ? name : name.Singularize())}";
        }

        public static string GetCodeTypeClassName(this Type type, params string[] genericParameters) =>
            type.FullName!.Split('`').First() + (genericParameters.Any() ? $"<{string.Join(",", genericParameters)}>" : string.Empty);

        public static string AsValidCSharpCode<T>(this T input)
        {
            using var stringWriter = new StringWriter();
            using var csharpCodeProvider = new CSharpCodeProvider();

            csharpCodeProvider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), stringWriter, null);

            return stringWriter.ToString();
        }
    }
}
