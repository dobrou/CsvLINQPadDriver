using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using CsvLINQPadDriver.CodeGen;
using CsvLINQPadDriver.Extensions;

#if NETCOREAPP
using System.Collections.Immutable;
#else
using CsvLINQPadDriver.Bcl.Extensions;
#endif

namespace CsvLINQPadDriver.DataModel
{
    internal class CsvDataModelGenerator
    {
        private const StringComparison IdsComparison = StringComparison.OrdinalIgnoreCase;

        private static readonly StringComparer IdsComparer = StringComparer.OrdinalIgnoreCase;

        private readonly ICsvDataContextDriverProperties _csvDataContextDriverProperties;

        private CsvDataModelGenerator(ICsvDataContextDriverProperties csvDataContextDriverProperties) =>
            _csvDataContextDriverProperties = csvDataContextDriverProperties;

        public static CsvDatabase CreateModel(ICsvDataContextDriverProperties csvDataContextDriverProperties) =>
            new CsvDataModelGenerator(csvDataContextDriverProperties).CreateModel();

        private CsvDatabase CreateModel()
        {
            var exceptions = new List<Exception>();

            var files = _csvDataContextDriverProperties.ParsedFiles.EnumFiles(exceptions)
                .OrderFiles(_csvDataContextDriverProperties.FilesOrderBy)
                .ToImmutableList();

            var baseDir = files.GetLongestCommonPrefixPath();

            var csvDatabase = new CsvDatabase(baseDir, CreateTables().ToImmutableList(), files, exceptions);

            MakeCodeNamesUnique(csvDatabase.Tables);

            foreach (var table in csvDatabase.Tables)
            {
                MakeCodeNamesUnique(table.Columns);
            }

            if (_csvDataContextDriverProperties.DetectRelations)
            {
                DetectRelations(csvDatabase);

                foreach (var table in csvDatabase.Tables)
                {
                    MakeCodeNamesUnique(table.Relations, table.Columns.Select(static csvColumn => csvColumn.CodeName!));
                }
            }

            UpdateDisplayNames(csvDatabase.Tables);
            UpdateDisplayNames(csvDatabase.Tables.SelectMany(static csvTable => csvTable.Columns));
            UpdateDisplayNames(csvDatabase.Tables.SelectMany(static csvTable => csvTable.Relations));

            return csvDatabase;

            IEnumerable<CsvTable> CreateTables()
            {
                var tableCodeNames = new Dictionary<string, string>();

                foreach (var file in files.Where(File.Exists))
                {
                    var doNotLockFiles = _csvDataContextDriverProperties.DoNotLockFiles;
                    var debugInfo = _csvDataContextDriverProperties.DebugInfo;
                    var csvSeparator  = _csvDataContextDriverProperties.UseCsvHelperSeparatorAutoDetection
                            ? null
                            : _csvDataContextDriverProperties.SafeCsvSeparator
                              ?? file.CsvDetectSeparator(doNotLockFiles, debugInfo).ToString();
                    var noBomEncoding = _csvDataContextDriverProperties.NoBomEncoding;
                    var allowComments = _csvDataContextDriverProperties.AllowComments;
                    var commentChar = _csvDataContextDriverProperties.CommentChar;
                    var ignoreBadData = _csvDataContextDriverProperties.IgnoreBadData;
                    var autoDetectEncoding = _csvDataContextDriverProperties.AutoDetectEncoding;
                    var ignoreBlankLines = _csvDataContextDriverProperties.IgnoreBlankLines;
                    var addHeader = _csvDataContextDriverProperties.AddHeader;
                    var headerDetection = _csvDataContextDriverProperties.HeaderDetection;
                    var headerFormat = _csvDataContextDriverProperties.HeaderFormat;
                    var whitespaceTrimOptions = _csvDataContextDriverProperties.WhitespaceTrimOptions;

                    if (_csvDataContextDriverProperties.IgnoreInvalidFiles &&
                        !file.IsCsvFormatValid(
                            csvSeparator,
                            noBomEncoding,
                            allowComments,
                            commentChar,
                            ignoreBadData,
                            autoDetectEncoding,
                            ignoreBlankLines,
                            doNotLockFiles,
                            debugInfo,
                            whitespaceTrimOptions))
                    {
                        exceptions.Add(file, "has invalid CSV format");
                        continue;
                    }

                    var fileName = Path.GetFileName(file);
                    var fileDir  = (Path.GetDirectoryName($"{file.Remove(0, baseDir.Length)}x") ?? string.Empty).TrimStart(Path.DirectorySeparatorChar);
                    var codeName = (Path.GetFileNameWithoutExtension(fileName) + (string.IsNullOrWhiteSpace(fileDir) ? string.Empty : $"_{fileDir}")).GetSafeCodeName();

#if NETCOREAPP
                    ImmutableList<CsvColumn> columns;
#else
                    List<CsvColumn> columns;
#endif

                    try
                    {
                        columns = file.CsvReadHeader(
                                csvSeparator,
                                noBomEncoding,
                                allowComments,
                                commentChar,
                                ignoreBadData,
                                autoDetectEncoding,
                                ignoreBlankLines,
                                doNotLockFiles,
                                addHeader,
                                headerDetection,
                                headerFormat,
                                whitespaceTrimOptions)
                            .Select(static (value, index) => (value, index))
                            .Select(static col => new CsvColumn(col.value ?? string.Empty, col.index)
                            {
                                CodeName = col.value.GetSafeCodeName(),
                                DisplayName = string.Empty
                            })
                            .ToImmutableList();
                    }
                    catch (Exception exception) when (exception.CanBeHandled())
                    {
                        exceptions.Add(file, exception);
                        continue;
                    }

                    if (!columns.Any())
                    {
                        exceptions.Add(file, "has no columns");
                        continue;
                    }

                    yield return new CsvTable(file, csvSeparator, columns, new List<CsvRelation>())
                    {
                        CodeName    = codeName,
                        ClassName   = GetClassName(),
                        DisplayName = $"{fileName}{(string.IsNullOrWhiteSpace(fileDir) ? string.Empty : $" in {fileDir}")} {file.GetHumanizedFileSize(debugInfo)}"
                    };

                    string? GetClassName()
                    {
                        if (!_csvDataContextDriverProperties.UseSingleClassForSameFiles)
                        {
                            return null;
                        }

                        var key = string.Join(string.Empty, columns.Select(static csvColumn => $"{csvColumn.Name}\t{csvColumn.Index}\n"));

                        if (!tableCodeNames.TryGetValue(key, out var className))
                        {
                            className = codeName;
                            tableCodeNames.Add(key, className);
                        }

                        return className;
                    }
                }
            }

            static void UpdateDisplayNames<TItem>(IEnumerable<TItem> items)
                where TItem : class, ICsvNames
            {
                foreach (var item in items)
                {
                    item.DisplayName = $"{item.CodeName}{(string.IsNullOrWhiteSpace(item.DisplayName) ? string.Empty : $" ({item.DisplayName})")}";
                }
            }
        }

        private static void MakeCodeNamesUnique<TItem>(IEnumerable<TItem> items, IEnumerable<string>? reservedNames = null)
            where TItem: class, ICsvNames
        {
            var names = new HashSet<string>(reservedNames ?? Enumerable.Empty<string>(), CsvTableBase.StringComparer);

            foreach (var item in items)
            {
                var name = item.CodeName!;
                if (names.Contains(name))
                {
                    // Get first unique name.
                    name = Enumerable.Range(1, int.MaxValue)
                            .Select(static i => i.ToString(CultureInfo.InvariantCulture))
                            .Select(s => $"{name}_{s}") // 1, 2, 3, 4...
                            .First(firstName => !names.Contains(firstName));
                    item.CodeName = name;
                }
                names.Add(name);
            }
        }

        private static IEnumerable<string> GetTableForeignKeyPossibleNames(CsvTable table)
        {
            var fileName = Path.GetFileNameWithoutExtension(table.FilePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                yield break;
            }

            // car -> carID
            yield return $"{fileName}id";

            // cars -> carID
            if (GetIdName("s", out var idsName))
            {
                yield return idsName!;
            }

            // buses -> busID
            if (GetIdName("es", out var idesName))
            {
                yield return idesName!;
            }

            bool GetIdName(string postfix, out string? idName)
            {
                if (fileName.EndsWith(postfix, IdsComparison))
                {
                    idName = $"{fileName[..^postfix.Length]}id";
                    return true;
                }

                idName = null;

                return false;
            }
        }

        private static void DetectRelations(CsvDatabase csvDatabase)
        {
            // Limit maximum relations count.
            var csvTables = csvDatabase.Tables;
            var maximumRelationsCount = csvTables.Count * csvTables.Count;

            var stringToCsvTableColumnLookup = (
                from csvTable in csvTables 
                from csvColumn in csvTable.Columns 
                where IsIdColumn(csvColumn.Name)
                select (csvTable, csvColumn)
            ).ToLookup(
                static csvTableColumn => csvTableColumn.csvColumn.Name,
                static tableColumn => tableColumn,
                IdsComparer);

            // t1.nameID -> name.ID
            // t1.nameID -> names.ID
            // t1.nameID -> name.nameID
            // t1.nameID -> names.nameID
            // t1.bookID -> books.authorID
            // t1.bookID -> books.ID
            var csvTableColumns = from csvTable in csvTables
                let keyNamesForeign = GetTableForeignKeyPossibleNames(csvTable)
                let keyNames = keyNamesForeign.Concat(new []{ "id" })
                from csvColumn in csvTable.Columns
                where keyNames.Contains(csvColumn.Name, IdsComparer)
                from csvTableColumn in keyNamesForeign.SelectMany(k => stringToCsvTableColumnLookup[k])
                where csvTableColumn.csvTable != csvTable
                select (csvTable1: csvTable, csvColumn1: csvColumn, csvTable2: csvTableColumn.csvTable, csvColumn2: csvTableColumn.csvColumn);

            // Translate to relations.
            var relations = from relation in
                (
                    from csvTableColumn in csvTableColumns.Take(maximumRelationsCount)
                    // Add reverse direction.
                    select new[] { csvTableColumn, (csvTable1: csvTableColumn.csvTable2, csvColumn1: csvTableColumn.csvColumn2, csvTable2: csvTableColumn.csvTable1, csvColumn2: csvTableColumn.csvColumn1) }
                ).SelectMany(static relation => relation).Distinct()
                select new CsvRelation(relation.csvTable1, relation.csvTable2, relation.csvColumn1, relation.csvColumn2)
                {
                    CodeName = relation.csvTable2.CodeName
                };

            // Add relations to DB structure.
            foreach (var relationsGroup in relations.GroupBy(static r => r.SourceTable))
            {
                foreach (var relation in relationsGroup)
                {
                    relationsGroup.Key.Relations.Add(relation);
                }
            }

            static bool IsIdColumn(string columnName) =>
                columnName.EndsWith("id", IdsComparison);
        }
    }
}
