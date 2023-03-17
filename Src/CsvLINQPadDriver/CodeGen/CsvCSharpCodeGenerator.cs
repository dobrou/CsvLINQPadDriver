using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;

using Microsoft.CSharp;

using Humanizer;

using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.DataModel;

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

        private const string NullableReferenceTypeSign =
#if NETCOREAPP
            "?"
#else
            "/*?*/"
#endif
        ;

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
        public static Result GenerateCode(CsvDatabase db, ref string nameSpace, ref string typeName, ICsvDataContextDriverProperties properties, Stopwatch stopwatch) =>
            new CsvCSharpCodeGenerator(nameSpace, typeName = DefaultContextTypeName, properties).GenerateSrcFile(db, stopwatch);

        private Result GenerateSrcFile(CsvDatabase csvDatabase, Stopwatch stopwatch)
        {
            var csvTables = csvDatabase.Tables;

            var groups = csvTables
                    .Select(table => GenerateTableRowDataTypeClass(table, _properties.UseRecordType, _properties.StringComparison, _properties.HideRelationsFromDump))
                    .GroupBy(static typeCode => typeCode.TypeName)
                    .ToImmutableList();

            var isStringInternEnabled = _properties.IsStringInternEnabled;

            return new Result($@"namespace {_contextNameSpace}
{{
    /// <summary>CSV Data Context</summary>
    public class {_contextTypeName} : {typeof(CsvDataContextBase).GetCodeTypeClassName()}
    {{{string.Join(string.Empty, csvTables.Select(static table => $@"
        /// <summary>File: {SecurityElement.Escape(table.FilePath)}</summary>
        public {typeof(CsvTableBase<>).GetCodeTypeClassName(GetClassName(table))} {table.CodeName} {{ get; private set; }}")
                )}

        public {_contextTypeName}()
        {{
            // Init tables data{string.Join(string.Empty, csvTables.Select(table => (Table: table, ClassName: GetClassName(table))).Select(context => $@"
            this.{context.Table.CodeName} = {typeof(CsvTableFactory).GetCodeTypeClassName()}.CreateTable<{context.ClassName}>(
                {ParamName("isStringInternEnabled")}{GetBoolConst(isStringInternEnabled)},
                {ParamName("internStringComparer")}{GetNullableValue(isStringInternEnabled && _properties.UseStringComparerForStringIntern, () => GetStringComparer(_properties.StringComparison))},
                {ParamName("isCacheEnabled")}{GetBoolConst(_properties.IsCacheEnabled)},
                {ParamName("csvSeparator")}{context.Table.CsvSeparator.AsValidCSharpCode()},
                {ParamName("noBomEncoding")}{typeof(NoBomEncoding).GetCodeTypeClassName()}.{_properties.NoBomEncoding},
                {ParamName("allowComments")}{GetBoolConst(_properties.AllowComments)},
                {ParamName("commentChar")}{GetNullableValue(_properties.AllowComments && _properties.CommentChar.HasValue, () => _properties.CommentChar.AsValidCSharpCode())},
                {ParamName("ignoreBadData")}{GetBoolConst(_properties.IgnoreBadData)},
                {ParamName("autoDetectEncoding")}{GetBoolConst(_properties.AutoDetectEncoding)},
                {ParamName("ignoreBlankLines")}{GetBoolConst(_properties.IgnoreBlankLines)},
                {ParamName("doNotLockFiles")}{GetBoolConst(_properties.DoNotLockFiles)},
                {ParamName("addHeader")}{GetBoolConst(_properties.AddHeader)},
                {ParamName("headerDetection")}{GetNullableValue(_properties.AddHeader, () => $"{typeof(HeaderDetection).GetCodeTypeClassName()}.{_properties.HeaderDetection}")},
                {ParamName("whitespaceTrimOptions")}{GetNullableValue(_properties.TrimSpaces, () => $"{typeof(WhitespaceTrimOptions).GetCodeTypeClassName()}.{_properties.WhitespaceTrimOptions}")},
                {ParamName("allowSkipLeadingRows")}{GetBoolConst(_properties.AllowSkipLeadingRows)},
                {ParamName("skipLeadingRowsCount")}{IntToStr(_properties.SkipLeadingRowsCount)},
                {ParamName("filePath")}{context.Table.FilePath.AsValidCSharpCode()},
                {ParamName("propertiesInfo")}new {typeof(CsvColumnInfoList).GetCodeTypeClassName()} {{
                    {string.Join(string.Empty, string.Join(@",
                    ", context.Table.Columns.Select(csvColumn => $@"{{ {IntToString(csvColumn.Index)}, {NameOf(context.ClassName, csvColumn.CodeName!)} }}")))}
                }},
                {ParamName("relationsInit")}r => {{{string.Join(string.Empty, context.Table.Relations.Select(csvRelation => (Relation: csvRelation, ClassName: GetClassName(csvRelation.TargetTable))).Select(static context => $@"
                    r.{context.Relation.CodeName} = new {typeof(LazyEnumerable<>).GetCodeTypeClassName(context.ClassName)}(
                        () => {context.Relation.TargetTable.CodeName}.WhereIndexed(tr => tr.{context.Relation.TargetColumn.CodeName}, {NameOf(context.ClassName, context.Relation.TargetColumn.CodeName!)}, r.{context.Relation.SourceColumn.CodeName}));"))}
                }}
            );")
                )}
        }}
    }} // context class

    // Data types{string.Join(Environment.NewLine, groups.Select(static grouping => grouping.OrderByDescending(code => code.Code.Length).First().Code))} // data types
}} // namespace
// {stopwatch.Elapsed}
// .NET {Environment.Version}
", groups);

            static string GetBoolConst(bool value) =>
                value ? "true" : "false";

            static string GetNullableValue(bool hasValue, Func<string> valueProvider) =>
                hasValue ? valueProvider() : "null";

            static string IntToStr(int value) =>
                value.ToString(Extensions.StringExtensions.DefaultCultureInfo);

            static string ParamName(string name) =>
                $"{name}: ";

            static string NameOf(string tableName, string name) =>
#if NETCOREAPP
                $"nameof({tableName}.{name})";
#else
                $@"""{name}"" /* {tableName}.{name} */";
#endif
        }

        private static TypeCodeResult GenerateTableRowDataTypeClass(CsvTable table, bool useRecordType, StringComparison stringComparison, bool hideRelationsFromDump)
        {
            var className = GetClassName(table);
            var properties = table.Columns.Select(GetPropertyName).ToImmutableList();

            var (generatedType, interfaces) =
#if NETCOREAPP
                useRecordType ? ("record", string.Empty) :
#endif
                ("class", $", {nameof(System)}.{nameof(IEquatable<object>)}<{className}>");

            return new TypeCodeResult(className, $@"
    public sealed {generatedType} {className} : {typeof(ICsvRowBase).GetCodeTypeClassName()}{interfaces}
    {{{string.Join(string.Empty, table.Columns.Select(static csvColumn => $@"
        public string{NullableReferenceTypeSign} {GetPropertyName(csvColumn)} {{ get; set; }}"))}
{GenerateIndexer(properties, "int",    static (_, index) => IntToString(index),  "at index {0}")}
{GenerateIndexer(properties, "string", static (property, _) => NameOf(property), "with name \\\"{0}\\\"")}
{GenerateToString(properties)}
{GenerateEqualsAndGetHashCode(className, useRecordType, stringComparison, properties)}{string.Join(string.Empty, table.Relations.Select(csvRelation => $@"

        /// <summary>{SecurityElement.Escape(csvRelation.DisplayName)}</summary>{(hideRelationsFromDump ? $@"
        [{typeof(HideFromDumpAttribute).GetCodeTypeClassName()}]" : string.Empty)}
        public {nameof(System)}.{nameof(System.Collections)}.{nameof(System.Collections.Generic)}.{nameof(IEnumerable)}<{csvRelation.TargetTable.GetCodeRowClassName()}>{NullableReferenceTypeSign} {csvRelation.CodeName} {{ get; set; }}")
            )}
    }}", table.CodeName!, table.FilePath);

            static string GetPropertyName(ICsvNames csvColumn) =>
                csvColumn.CodeName!;

            static string NameOf(string name) =>
#if NETCOREAPP
                $"nameof({name})";
#else
                $@"""{name}""";
#endif
        }

        private static string GenerateIndexer(IReadOnlyCollection<string> properties, string indexerType, Func<string, int, string> caseGetter, string exceptionMessage)
        {
            return $@"
        [{typeof(HideFromDumpAttribute).GetCodeTypeClassName()}]
        public string{NullableReferenceTypeSign} this[{indexerType} index]
        {{
            get
            {{
                switch(index)
                {{{string.Join(string.Empty, properties.Select((property, index) => $@"
                    case {caseGetter(property, index)}: return {property};"))}
                    {GenerateIndexerException()}
                }}
            }}
            set
            {{
                switch(index)
                {{{string.Join(string.Empty, properties.Select((property, index) => $@"
                    case {caseGetter(property, index)}: {property} = value; return;"))}
                    {GenerateIndexerException()}
                }}
            }}
        }}";

            string GenerateIndexerException() =>
                $@"default: throw new {nameof(System)}.{nameof(IndexOutOfRangeException)}(string.Format(""There is no property {exceptionMessage}"", index));";
        }

        private static string IntToString(int val) =>
            val.ToString(CultureInfo.InvariantCulture);

        private static string GenerateToString(IReadOnlyCollection<string> properties)
        {
            var namePadding = properties.Max(static property => property.Length);

            return $@"
        public override string{NullableReferenceTypeSign} ToString()
        {{
            return string.Format({string.Join(" +", properties.Select((property, index) => $@"
                ""{property.PadRight(namePadding)} : {{{IntToString(index + 1)}}}{{0}}"""
            ))},
                {nameof(System)}.{nameof(Environment)}.{nameof(Environment.NewLine)},
                {string.Join(@",
                ", properties)});
        }}";
        }

        private static string GenerateEqualsAndGetHashCode(string typeName, bool useRecordType, StringComparison stringComparison, IReadOnlyCollection<string> properties)
        {
            var nullableTypeName = typeName + NullableReferenceTypeSign;

            var objectEquals = useRecordType
                ? string.Empty
                : $@"
        public override bool Equals(object{NullableReferenceTypeSign} obj)
        {{
            if(obj == null || obj.GetType() != typeof({typeName})) return false;
            return Equals(({nullableTypeName})obj);
        }}

        public static bool operator == ({nullableTypeName} obj1, {nullableTypeName} obj2)
        {{
            return ReferenceEquals(obj1, null)
                ? ReferenceEquals(obj2, null)
                : obj1.Equals(obj2);
        }}

        public static bool operator != ({nullableTypeName} obj1, {nullableTypeName} obj2)
        {{
            return !(obj1 == obj2);
        }}
";

            return $@"{objectEquals}
        public bool Equals({nullableTypeName} obj)
        {{
            if(obj == null) return false;
            if(ReferenceEquals(this, obj)) return true;
            return  {string.Join($" &&{Environment.NewLine}{GetIndent(20)}",
                properties.Select(GetStringEquals))};
        }}

        public override int GetHashCode()
        {{
            var hashCode = new {nameof(System)}.{nameof(HashCode)}();

{string.Join(Environment.NewLine, properties.Select(property => $"{GetIndent(12)}hashCode.Add({property}, {GetStringComparer(stringComparison)});"))}

            return hashCode.ToHashCode();
        }}";

            string GetStringEquals(string property) =>
#if NETCOREAPP
                $"{GetStringComparer(stringComparison)}.Equals({property}, obj.{property})";
#else
                $"string.Equals({property}, obj.{property}, {nameof(System)}.{nameof(StringComparison)}.{stringComparison})";
#endif
        }

        private static string GetClassName(CsvTable table) =>
            table.GetCodeRowClassName();

        private static string GetIndent(int count) =>
            new(' ', count);

        private static string GetStringComparer(StringComparison stringComparison) =>
            $"{nameof(System)}.{nameof(StringComparer)}." + stringComparison switch
            {
                StringComparison.CurrentCulture             => nameof(StringComparer.CurrentCulture),
                StringComparison.CurrentCultureIgnoreCase   => nameof(StringComparer.CurrentCultureIgnoreCase),
                StringComparison.InvariantCulture           => nameof(StringComparer.InvariantCulture),
                StringComparison.InvariantCultureIgnoreCase => nameof(StringComparer.InvariantCultureIgnoreCase),
                StringComparison.Ordinal                    => nameof(StringComparer.Ordinal),
                StringComparison.OrdinalIgnoreCase          => nameof(StringComparer.OrdinalIgnoreCase),
                _                                           => throw new IndexOutOfRangeException($"Unknown {nameof(StringComparison)} {stringComparison}")
            };
    }

    internal static class CsvCSharpCodeGeneratorExtensions
    {
        public static string GetCodeRowClassName(this CsvTable table)
        {
            return ToClassName(table.ClassName);

            static string ToClassName(string? name) =>
                string.IsNullOrWhiteSpace(name)
                    ? throw new NullReferenceException("Class name is null or whitespace")
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
