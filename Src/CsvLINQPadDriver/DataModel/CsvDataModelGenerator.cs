using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;

using CsvLINQPadDriver.CodeGen;
using CsvLINQPadDriver.Helpers;

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
            var files = _csvDataContextDriverProperties.ParsedFiles.EnumFiles()
                .OrderFiles(_csvDataContextDriverProperties.FilesOrderBy)
                .ToImmutableList();

            var baseDir = files.GetLongestCommonPrefixPath();

            var csvDatabase = new CsvDatabase(baseDir, CreateTables().ToImmutableList(), files);

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
                    MakeCodeNamesUnique(table.Relations, table.Columns.Select(c => c.CodeName!));
                }
            }

            UpdateDisplayNames(csvDatabase.Tables);
            UpdateDisplayNames(csvDatabase.Tables.SelectMany(csvTable => csvTable.Columns));
            UpdateDisplayNames(csvDatabase.Tables.SelectMany(csvTable => csvTable.Relations));

            return csvDatabase;

            IEnumerable<CsvTable> CreateTables()
            {
                var tableCodeNames = new Dictionary<string, string>();

                foreach (var file in files.Where(File.Exists))
                {
                    var csvSeparator  = _csvDataContextDriverProperties.CsvSeparatorChar;
                    var noBomEncoding = _csvDataContextDriverProperties.NoBomEncoding;
                    var allowComments = _csvDataContextDriverProperties.AllowComments;

                    if (_csvDataContextDriverProperties.IgnoreInvalidFiles &&
                        !file.IsCsvFormatValid(csvSeparator, noBomEncoding, allowComments))
                    {
                        continue;
                    }

                    var fileName = Path.GetFileName(file);
                    var fileDir  = (Path.GetDirectoryName($"{file.Remove(0, baseDir.Length)}x") ?? string.Empty).TrimStart(Path.DirectorySeparatorChar);
                    var codeName = CodeGenHelper.GetSafeCodeName(Path.GetFileNameWithoutExtension(fileName) + (string.IsNullOrWhiteSpace(fileDir) ? string.Empty : $"_{fileDir}"));

                    var columns = file.CsvReadHeader(csvSeparator, noBomEncoding, allowComments)
                        .Select((value, index) => (value, index))
                        .Select(col => new CsvColumn(col.value ?? string.Empty, col.index)
                        {
                            CodeName    = CodeGenHelper.GetSafeCodeName(col.value),
                            DisplayName = string.Empty
                        })
                        .ToImmutableList();

                    yield return new CsvTable(file, csvSeparator, columns, new List<CsvRelation>())
                    {
                        CodeName    = codeName,
                        ClassName   = GetClassName(),
                        DisplayName = $"{fileName}{(string.IsNullOrWhiteSpace(fileDir) ? string.Empty : $" in {fileDir}")} {file.GetHumanizedFileSize()}"
                    };

                    string? GetClassName()
                    {
                        if (!_csvDataContextDriverProperties.UseSingleClassForSameFiles)
                        {
                            return null;
                        }

                        var key = string.Join(string.Empty, columns.Select(c => $"{c.Name}\t{c.Index}\n"));

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
                            .Select(i => i.ToString(CultureInfo.InvariantCulture))
                            .Select(s => name + s) // 1, 2, 3, 4...
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

            // item -> itemID
            yield return $"{fileName}id";

            // items -> itemID
            if (GetIdName("s", out var idsName))
            {
                yield return idsName!;
            }

            // fishes -> fishID
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
                where csvColumn.Name.EndsWith("id", IdsComparison)
                select (csvTable, csvColumn)
            ).ToLookup(csvTableColumn => csvTableColumn.csvColumn.Name, tableColumn => tableColumn, IdsComparer);

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
                ).SelectMany(relation => relation).Distinct()
                select new CsvRelation(relation.csvTable1, relation.csvTable2, relation.csvColumn1, relation.csvColumn2)
                {
                    CodeName = relation.csvTable2.CodeName
                };

            // Add relations to DB structure.
            foreach (var relationsGroup in relations.GroupBy(r => r.SourceTable))
            {
                foreach (var relation in relationsGroup)
                {
                    relationsGroup.Key.Relations.Add(relation);
                }
            }
        }
    }
}
