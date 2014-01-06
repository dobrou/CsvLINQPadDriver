using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver.DataModel
{
    public class CsvDataModelGenerator
    {

        /// <summary>
        /// Create CSV DB model based on config - mainly CSV files dir.
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public static CsvDatabase CreateModel(CsvDataContextDriverProperties props)
        {
            return new CsvDataModelGenerator(props).CreateModel();
        }

        protected CsvDataContextDriverProperties Properties;

        public CsvDataModelGenerator(CsvDataContextDriverProperties properties)
        {
            this.Properties = properties;
        }

        public CsvDatabase CreateModel() {
            string[] files = FileUtils.EnumFiles(Properties.Files.Split('\n'));
         
            string baseDir = FileUtils.GetLongestCommonPrefixPath(files);

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
                        CodeName = GetSafeCodeName(Path.GetFileNameWithoutExtension(fileName) + (string.IsNullOrWhiteSpace(fileDir) ? "" : ("_" + fileDir))),
                        DisplayName = fileName + (string.IsNullOrWhiteSpace(fileDir) ? "" : (" in " + fileDir)) + " (" + FileUtils.GetFileSizeInfo(file) + ")",
                        CsvSeparator = csvSeparator,
                        Columns = (
                            from col in FileUtils.CsvReadHeader(file, csvSeparator).Select((value,index) => new { value, index })
                            select new CsvColumn() {
                                DisplayName = col.value ?? "",
                                CodeName = GetSafeCodeName(col.value),
                                CsvColumnName = col.value ?? "",
                                CsvColumnIndex = col.index,
                            }
                        ).ToList(),
                    }
                ).ToList(),
            };

            MakeCodeNamesUnique(db);

            if (Properties.DetectRelations)
            {
                DetectRelations(db);
            }

            return db;
        }

        protected void MakeCodeNamesUnique(CsvDatabase db)
        {
            //TODO
            /*
            var tablesWithDupNames = db.Tables
                .ToLookup(table => table.CodeName)
                .Where(tgroup => tgroup.Count() > 1)
                ;
            foreach (var tables in tablesWithDupNames)
            {
                tables.ToList().ForEach();
            }
             */
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
                        from r in (new[] { r1 }).SelectMany(r => r)
                        select new[] { r, new { t1 = r.t2, c1 = r.c2, t2 = r.t1, c2 = r.c1 } } //add reverse direction
                    ).SelectMany(r => r).Distinct()
                select new CsvRelation()
                {
                    CodeName = r.t2.CodeName + "_" + r.c2.CodeName + "_" + r.c1.CodeName,
                    DisplayName = r.t2.DisplayName + " (" + r.c2.DisplayName + "==" + r.c1.DisplayName + ")",
                    SourceTable = r.t1,
                    SourceColumn = r.c1,
                    TargetTable = r.t2,
                    TargetColumn = r.c2,
                }
            );

            foreach (var relationsGroup in relations.GroupBy(r => r.SourceTable))
            {
                relationsGroup.Key.Relations.AddRange(relationsGroup);
            }
        }

        private static readonly Regex codeNameInvalidCharacters = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]", RegexOptions.Compiled);
        private const string safeChar = "_";
        private const int maxLength = 128;
        private CodeDomProvider csCodeProvider = Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#");
        protected string GetSafeCodeName(string name)
        {
            string safeName = name ?? "";

            if (safeName.Length > maxLength)
                safeName = safeName.Substring(0, maxLength);
            
            safeName = codeNameInvalidCharacters.Replace(safeName, safeChar);
            safeName = Regex.Replace(safeName, safeChar+"+",safeChar);
            safeName = Regex.Replace(safeName, "^"+safeChar+"+", "");

            if (string.IsNullOrEmpty(safeName))
                return safeChar + "empty";

            if (!char.IsLetter(safeName, 0))
                safeName = safeChar + safeName;

            if (!csCodeProvider.IsValidIdentifier(safeName))
                safeName = safeName + safeChar;
            
            return safeName;
        }

    }
}
