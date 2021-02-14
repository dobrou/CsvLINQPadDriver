using CsvLINQPadDriver.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CsvLINQPadDriver.DataModel
{
    public class CsvDataModelGenerator
    {

        /// <summary>
        /// Create CSV DB model based on config - mainly CSV files dir.
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static CsvDatabase CreateModel(ICsvDataContextDriverProperties props)
        {
            return new CsvDataModelGenerator(props).CreateModel();
        }

        protected ICsvDataContextDriverProperties Properties;

        public CsvDataModelGenerator(ICsvDataContextDriverProperties properties)
        {
            this.Properties = properties;
        }

        public CsvDatabase CreateModel() 
        {
            string[] files = FileUtils.EnumFiles(Properties.Files.Split('\n')).Select(f => f.Trim()).ToArray();
            string baseDir = FileUtils.GetLongestCommonPrefixPath(files);

            //create db structure
            var db = new CsvDatabase() {
                Name = baseDir,
                Tables = (
                    from file in files
                    where File.Exists(file)
                    let csvSeparator = Properties.CsvSeparatorChar ?? FileUtils.CsvDetectSeparator(file)
                    where !Properties.IgnoreInvalidFiles || FileUtils.CsvIsFormatValid(file, csvSeparator)
                    let fileName = Path.GetFileName(file)
                    let fileDir = (Path.GetDirectoryName(file.Remove(0, baseDir.Length)+"x")??"").TrimStart(Path.DirectorySeparatorChar)
                    select new CsvTable() {
                        FilePath = file,
                        CodeName = CodeGenHelper.GetSafeCodeName(Path.GetFileNameWithoutExtension(fileName) + (string.IsNullOrWhiteSpace(fileDir) ? "" : ("_" + fileDir))),
                        DisplayName = fileName + (string.IsNullOrWhiteSpace(fileDir) ? "" : (" in " + fileDir)) + " " + FileUtils.GetFileSizeInfo(file) + "",
                        CsvSeparator = csvSeparator,
                        Columns = (
                            from col in FileUtils.CsvReadHeader(file, csvSeparator).Select((value,index) => new { value, index })
                            select new CsvColumn() {
                                CodeName = CodeGenHelper.GetSafeCodeName(col.value),
                                DisplayName = "",
                                CsvColumnName = col.value ?? "",
                                CsvColumnIndex = col.index,
                            }
                        ).ToList(),
                    }
                ).ToList(),
            };

            //unique code names
            MakeCodeNamesUnique(db.Tables, t => t.CodeName, (t, n) => t.CodeName = n);
            foreach (var table in db.Tables)
            {
                MakeCodeNamesUnique(table.Columns, c => c.CodeName, (c, n) => c.CodeName = n);
            }
            
            //relations
            if (Properties.DetectRelations)
            {
                DetectRelations(db);
                foreach (var table in db.Tables)
                {
                    MakeCodeNamesUnique(table.Relations, c => c.CodeName, (c, n) => c.CodeName = n, table.Columns.Select(c => c.CodeName));
                }            

            }

            //adjust displaynames
            foreach (var x in db.Tables)                                x.DisplayName = x.CodeName + (string.IsNullOrWhiteSpace(x.DisplayName) ? "" : " (" + x.DisplayName + ")");
            foreach (var x in db.Tables.SelectMany(t => t.Columns))     x.DisplayName = x.CodeName + (string.IsNullOrWhiteSpace(x.DisplayName) ? "" : " (" + x.DisplayName + ")");
            foreach (var x in db.Tables.SelectMany(t => t.Relations))   x.DisplayName = x.CodeName + (string.IsNullOrWhiteSpace(x.DisplayName) ? "" : " (" + x.DisplayName + ")");

            return db;
        }

        /// <summary>
        /// Makes all names on items unique by adding default suffixes.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items"></param>
        /// <param name="nameGet"></param>
        /// <param name="nameSet"></param>
        /// <param name="reservedNames"></param>
        protected static void MakeCodeNamesUnique<TItem>(IEnumerable<TItem> items, Func<TItem, string> nameGet, Action<TItem, string> nameSet, IEnumerable<string> reservedNames = null)
        {
            var names = new HashSet<string>(reservedNames ?? Enumerable.Empty<string>(), StringComparer.Ordinal);

            foreach (var item in items)
            {
                string name = nameGet(item);
                if (names.Contains(name))
                {
                    //get first unique name
                    name = Enumerable.Range(1, int.MaxValue)
                            .Select(i => i.ToString(CultureInfo.InvariantCulture))
                            .Select(s => name + s) //1,2,3,4...
                            .First(nname => !names.Contains(nname));
                    nameSet(item, name);
                }
                names.Add(name);
            }
        }

        protected IEnumerable<string> GetTableForeignKeyPossibleNames(CsvTable table)
        {
            string fileName = Path.GetFileNameWithoutExtension(table.FilePath);
            if(string.IsNullOrWhiteSpace(fileName))
                yield break;
            
            //item -> itemID
            yield return fileName + "id";

            //items -> itemID
            if (fileName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                yield return fileName.Substring(0, fileName.Length - 1) + "id";

            //fishes -> fishID
            if (fileName.EndsWith("es", StringComparison.OrdinalIgnoreCase))
                yield return fileName.Substring(0, fileName.Length - 2) + "id";
        }

        protected void DetectRelations(CsvDatabase db)
        {
            //limit maximum relations count
            int maximumRelationsCount = db.Tables.Count*db.Tables.Count;

            var tcl = (
                from tab in db.Tables 
                from col in tab.Columns 
                where col.CsvColumnName.EndsWith("id", StringComparison.OrdinalIgnoreCase)
                select new {tab, col}
            ).ToLookup(k => k.col.CsvColumnName, v => v, StringComparer.OrdinalIgnoreCase);

            // t1.nameID -> name.ID
            // t1.nameID -> names.ID
            // t1.nameID -> name.nameID
            // t1.nameID -> names.nameID
            // t1.fishID -> fishes.fishID
            // t1.fishID -> fishes.ID
            var r1 = (
                from t1 in db.Tables
                let keyNamesForeign = GetTableForeignKeyPossibleNames(t1)
                let keyNames = keyNamesForeign.Concat(new []{"id"})
                from c1 in t1.Columns
                where keyNames.Contains( c1.CsvColumnName, StringComparer.OrdinalIgnoreCase)

                from tc2 in keyNamesForeign.SelectMany(k => tcl[k])
                where tc2.tab != t1
                select new { t1, c1, t2 = tc2.tab, c2 = tc2.col }
            );

            //translate to relations            
            var relations = (
                from r in
                    (
                        from r in (new[] { r1 }).SelectMany(r => r).Take(maximumRelationsCount)
                        select new[] { r, new { t1 = r.t2, c1 = r.c2, t2 = r.t1, c2 = r.c1 } } //add reverse direction
                    ).SelectMany(r => r).Distinct()
                select new CsvRelation()
                {
                    CodeName = r.t2.CodeName,
                    SourceTable = r.t1,
                    SourceColumn = r.c1,
                    TargetTable = r.t2,
                    TargetColumn = r.c2,
                }
            );

            //add relations to DB structure
            int relationCount = 0;
            foreach (var relationsGroup in relations.GroupBy(r => r.SourceTable))
            {
                foreach (var relation in relationsGroup)
                {
                    relationsGroup.Key.Relations.Add(relation);
                    relationCount++;
                }                
            }

            Logger.Log("Relations detected {0} {1}", relationCount, relationCount >= maximumRelationsCount ? "Maximum limit reached" : "");
        }

    }
}
