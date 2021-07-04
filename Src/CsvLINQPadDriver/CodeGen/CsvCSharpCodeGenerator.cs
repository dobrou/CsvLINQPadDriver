using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;

using Microsoft.CSharp;

using Humanizer;

using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.DataModel;
using CsvLINQPadDriver.Extensions;

#if NETCOREAPP
using System.Collections.Immutable;
#else
using CsvLINQPadDriver.Bcl.Extensions;
#endif

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
        public static Result GenerateCode(CsvDatabase db, ref string nameSpace, ref string typeName, ICsvDataContextDriverProperties properties) =>
            new CsvCSharpCodeGenerator(nameSpace, typeName = DefaultContextTypeName, properties).GenerateSrcFile(db);

        private Result GenerateSrcFile(CsvDatabase csvDatabase)
        {
            var csvTables = csvDatabase.Tables;

            var groups = csvTables
                    .Select(table => GenerateTableRowDataTypeClass(table, _properties.UseRecordType, _properties.StringComparison, _properties.HideRelationsFromDump))
                    .GroupBy(typeCode => typeCode.TypeName)
                    .ToImmutableList();

            var isStringInternEnabled = _properties.IsStringInternEnabled;

            return new Result($@"namespace {_contextNameSpace}
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
                {GetBoolConst(isStringInternEnabled)},
                {(isStringInternEnabled && _properties.UseStringComparerForStringIntern ? GetStringComparer(_properties.StringComparison) : "null")},
                {GetBoolConst(_properties.IsCacheEnabled)},
                {table.CsvSeparator.AsValidCSharpCode()},
                {typeof(NoBomEncoding).GetCodeTypeClassName()}.{_properties.NoBomEncoding},
                {GetBoolConst(_properties.AllowComments)},
                {GetBoolConst(_properties.IgnoreBadData)},
                {GetBoolConst(_properties.AutoDetectEncoding)},
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

    // Data types {string.Join(Environment.NewLine, groups.Select(grouping => grouping.OrderByDescending(code => code.Code.Length).First().Code))} // data types
}} // namespace
", groups);

            static string GetBoolConst(bool val) =>
                val ? "true" : "false";
        }

        private static TypeCodeResult GenerateTableRowDataTypeClass(CsvTable table, bool useRecordType, StringComparison stringComparison, bool hideRelationsFromDump)
        {
            var className = GetClassName(table);
            var properties = table.Columns.Select(GetPropertyName).ToImmutableList();

            var (generatedType, interfaces) =
#if NETCOREAPP
                useRecordType ? ("record", string.Empty) :
#endif
                ("class", $", System.IEquatable<{className}>");

            return new TypeCodeResult(className, $@"
    public sealed {generatedType} {className} : {typeof(ICsvRowBase).GetCodeTypeClassName()}{interfaces}
    {{{string.Join(string.Empty, table.Columns.Select(csvColumn => $@"
        public string {GetPropertyName(csvColumn)} {{ get; set; }}"))}
{GenerateIndexer(properties, true)}
{GenerateIndexer(properties, false)}
{GenerateToString(properties)}
{GenerateEqualsAndGetHashCode(className, useRecordType, stringComparison, properties)}{string.Join(string.Empty, table.Relations.Select(csvRelation => $@"

        /// <summary>{SecurityElement.Escape(csvRelation.DisplayName)}</summary> {(hideRelationsFromDump ? $@"
        [{typeof(HideFromDumpAttribute).GetCodeTypeClassName()}]" : string.Empty)}
        public System.Collections.Generic.IEnumerable<{csvRelation.TargetTable.GetCodeRowClassName()}> {csvRelation.CodeName} {{ get; set; }}")
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
                $@"default: throw new System.IndexOutOfRangeException(string.Format(""There is no property {(intIndexer ? "at index {0}" : "with name \\\"{0}\\\"")}"", index));";

            static string IntToStr(int val) =>
                val.ToString(CultureInfo.InvariantCulture);
        }

        private static string GenerateToString(IReadOnlyCollection<string> properties)
        {
            var namePadding = properties.Max(property => property.Length);

            return $@"
        public override string ToString()
        {{
            return string.Format(""{string.Join(string.Empty, properties.Select((v, i) => $"{v.PadRight(namePadding)} : {{{i+1}}}{{0}}"))}"", System.Environment.NewLine, {string.Join(", ", properties)});
        }}";
        }

        private static string GenerateEqualsAndGetHashCode(string typeName, bool useRecordType, StringComparison stringComparison, IReadOnlyCollection<string> properties)
        {
            var objectEquals = useRecordType
                ? string.Empty
                : $@"
        public override bool Equals(object obj)
        {{
            if(obj == null || obj.GetType() != typeof({typeName})) return false;
            return Equals(({typeName})obj);
        }}

        public static bool operator == ({typeName} obj1, {typeName} obj2)
        {{
            return ReferenceEquals(obj1, null)
                ? ReferenceEquals(obj2, null)
                : obj1.Equals(obj2);
        }}

        public static bool operator != ({typeName} obj1, {typeName} obj2)
        {{
            return !(obj1 == obj2);
        }}
";

            return $@"{objectEquals}
        public bool Equals({typeName} obj)
        {{
            if(obj == null) return false;
            if(ReferenceEquals(this, obj)) return true;
            return  {string.Join($" && {Environment.NewLine}{GetIndent(20)}",
                properties.Select(property => $"{GetStringComparer(stringComparison)}.Equals({property}, obj.{property})"))};
        }}

        public override int GetHashCode()
        {{
            var hashCode = new System.HashCode();

{string.Join(Environment.NewLine, properties.Select(property => $"{GetIndent(12)}hashCode.Add({property}, {GetStringComparer(stringComparison)});"))}

            return hashCode.ToHashCode();
        }}";
        }

        private static string GetClassName(CsvTable table) =>
            table.GetCodeRowClassName();

        private static string GetIndent(int count) =>
            new(' ', count);

        private static string GetStringComparer(StringComparison stringComparison) =>
            "System.StringComparer." + stringComparison switch
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
                    : $"R{(name!.Length < 3 ? name : name.Singularize())}";
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
