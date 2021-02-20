using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CsvHelper;

using CsvLINQPadDriver.CodeGen;
using CsvLINQPadDriver.DataModel;

using LINQPad.Extensibility.DataContext;

namespace CsvLINQPadDriver
{
    internal class SchemaBuilder
    {
        internal static List<ExplorerItem> GetSchemaAndBuildAssembly(ICsvDataContextDriverProperties csvDataContextDriverProperties, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            var csvDatabase = CsvDataModelGenerator.CreateModel(csvDataContextDriverProperties);

            var code = CsvCSharpCodeGenerator.GenerateCode(csvDatabase, ref nameSpace, ref typeName, csvDataContextDriverProperties);

            var compileErrors = BuildAssembly(code, assemblyToBuild);

            var schema = GetSchema(csvDatabase, csvDataContextDriverProperties);

            var hasCompileErrors = compileErrors?.Any() == true;

            if (hasCompileErrors || csvDataContextDriverProperties.DebugInfo)
            {
                schema.Insert(0, new ExplorerItem("Context Source Code", ExplorerItemKind.Schema, ExplorerIcon.Schema)
                {
                    ToolTipText = "Data Context source code. Drag&drop to text window.",
                    DragText = code
                });
            }

            if (hasCompileErrors)
            {
                schema.Insert(0, new ExplorerItem("Context Compile ERROR", ExplorerItemKind.Schema, ExplorerIcon.Box)
                {
                    ToolTipText = "Data context compile failed. Drag&Drop error messages to text window to see them",
                    DragText = string.Join(Environment.NewLine, compileErrors)
                });
            }

            if (!csvDatabase.Tables.Any())
            {
                schema.Insert(0, new ExplorerItem("No files found", ExplorerItemKind.Schema, ExplorerIcon.Box));
            }

            return schema;
        }

        private static string[] BuildAssembly(string code, AssemblyName name)
        {
            var referencedAssemblies = DataContextDriver.GetCoreFxReferenceAssemblies().Concat(new []
            {
                typeof(SchemaBuilder).Assembly.Location,
                typeof(CsvReader).Assembly.Location
            });

            var result = DataContextDriver.CompileSource(new CompilationInput
            {
                FilePathsToReference = referencedAssemblies.ToArray(),
                OutputPath = name.CodeBase,
                SourceCode = new[] { code }
            });

            return result.Successful ? null : result.Errors;
        }

        private static List<ExplorerItem> GetSchema(CsvDatabase db, ICsvDataContextDriverProperties _) =>
            (db.Tables ?? Enumerable.Empty<CsvTable>()).Select(table =>
                new ExplorerItem(table.DisplayName, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                {
                    DragText = table.CodeName,
                    IsEnumerable = true,
                    ToolTipText = table.FilePath,
                    Children =
                        (table.Columns ?? Enumerable.Empty<CsvColumn>())
                            .Select(column =>
                                new ExplorerItem(column.DisplayName, ExplorerItemKind.Property, ExplorerIcon.Column)
                                {
                                    DragText = column.CodeName,
                                    ToolTipText = $"{column.CsvColumnIndex + 1}:{column.CsvColumnName}"
                                }
                            ).Concat(
                                table.Relations.Select(relation =>
                                    new ExplorerItem(relation.DisplayName, ExplorerItemKind.CollectionLink, ExplorerIcon.ManyToMany)
                                    {
                                        DragText = relation.CodeName,
                                        ToolTipText = $"Relation to {relation.TargetTable.CodeName} where {relation.SourceTable.CodeName}.{relation.SourceColumn.CodeName} == {relation.TargetTable.CodeName}.{relation.TargetColumn.CodeName}"
                                    })
                            ).ToList()
                }).ToList();
    }
}
