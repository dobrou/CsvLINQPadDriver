using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using CsvHelper;

using CsvLINQPadDriver.CodeGen;
using CsvLINQPadDriver.DataModel;
using CsvLINQPadDriver.Extensions;

using LINQPad.Extensibility.DataContext;

namespace CsvLINQPadDriver
{
    internal class SchemaBuilder
    {
        internal static List<ExplorerItem> GetSchemaAndBuildAssembly(ICsvDataContextDriverProperties csvDataContextDriverProperties, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            const ExplorerItemKind errorExplorerItemKind = ExplorerItemKind.ReferenceLink;
            const ExplorerIcon errorExplorerIcon = ExplorerIcon.Box;

            var csvDatabase = CsvDataModelGenerator.CreateModel(csvDataContextDriverProperties);

            var (code, tableCodeGroups) = CsvCSharpCodeGenerator.GenerateCode(csvDatabase, ref nameSpace, ref typeName, csvDataContextDriverProperties);

            var compileErrors = BuildAssembly(code, assemblyToBuild);
            var hasCompilationErrors = compileErrors.Any();

            var schema = GetSchema(csvDatabase);

            var index = 0;

            if (hasCompilationErrors || csvDataContextDriverProperties.DebugInfo)
            {
                schema.Insert(index++, new ExplorerItem("Data context source code", ExplorerItemKind.Schema, ExplorerIcon.Schema)
                {
                    ToolTipText = "Drag&drop context source code to text window",
                    DragText = code
                });
            }

            var exceptions = csvDatabase.Exceptions;
            if (exceptions.Any())
            {
                var fileOrFolder = $"{exceptions.Pluralize("file")} or {exceptions.Pluralize("folder")}";
                schema.Insert(index++, new ExplorerItem($"{exceptions.Count} {fileOrFolder} {exceptions.Pluralize("was", "were")} not processed", errorExplorerItemKind, errorExplorerIcon)
                {
                    ToolTipText = $"Drag&drop {fileOrFolder} processing {exceptions.Pluralize("error")} to text window",
                    DragText = exceptions.Select(exception => exception.Message).JoinNewLine()
                });
            }

            if (hasCompilationErrors)
            {
                schema.Insert(0, new ExplorerItem("Data context compilation failed", errorExplorerItemKind, errorExplorerIcon)
                {
                    ToolTipText = "Drag&drop data context compilation errors to text window",
                    DragText = compileErrors.JoinNewLine()
                });
            }
            else
            {
                foreach (var tableCodeGroup in tableCodeGroups.Where(codeGroup => codeGroup.Count() > 1))
                {
                    var codeNames = tableCodeGroup.Select(typeCodeResult => typeCodeResult.CodeName).ToImmutableList();
                    var similarFilesSize = tableCodeGroup.Select(typeCodeResult => typeCodeResult.FilePath).GetHumanizedFileSize();
                    var filePaths = new HashSet<string>(codeNames);
                    var similarFilesCount = codeNames.Count;

                    schema.Insert(index++, new ExplorerItem($"{codeNames.First()} similar files joined data ({similarFilesCount}/{csvDatabase.Files.Count} files {similarFilesSize})", ExplorerItemKind.QueryableObject, ExplorerIcon.View)
                    {
                        Children = schema.Where(IsSimilarFile).ToList(),
                        IsEnumerable = true,
                        ToolTipText =
                            $"Drag&drop {similarFilesCount} similar files joined data to text window".JoinNewLine(
                            string.Empty,
                            $"{string.Join(Environment.NewLine, similarFilesCount <= 3 ? codeNames : codeNames.Take(2).Concat(new []{ "..." }).Concat(codeNames.Skip(similarFilesCount - 1)))}"),
                        DragText = $@"new []
{{
{string.Join(Environment.NewLine, codeNames.Select(n => $"\t{n},"))}
}}.SelectMany(_ => _)
"
                    });

                    if (!csvDataContextDriverProperties.ShowSameFilesNonGrouped)
                    {
                        schema.RemoveAll(IsSimilarFile);
                    }

                    bool IsSimilarFile(ExplorerItem explorerItem) =>
                        filePaths!.Contains(explorerItem.Tag);
                }
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

            return result.Successful ? new string[0] : result.Errors;
        }

        private static List<ExplorerItem> GetSchema(CsvDatabase db) =>
            db.Tables.Select(table =>
                new ExplorerItem(table.DisplayName, ExplorerItemKind.QueryableObject, ExplorerIcon.Table)
                {
                    DragText = table.CodeName,
                    Tag = table.CodeName,
                    IsEnumerable = true,
                    ToolTipText = table.FilePath,
                    Children =
                        table.Columns
                            .Select(column =>
                                new ExplorerItem(column.DisplayName, ExplorerItemKind.Property, ExplorerIcon.Column)
                                {
                                    DragText = column.CodeName,
                                    ToolTipText = $"{column.Index}:{column.Name}"
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
