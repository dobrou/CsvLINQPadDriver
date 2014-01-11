using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
using CsvLINQPadDriver.CodeGen;
using CsvLINQPadDriver.DataModel;
using CsvLINQPadDriver.Helpers;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;

namespace CsvLINQPadDriver
{
    internal class SchemaBuilder
    {

        internal static List<ExplorerItem> GetSchemaAndBuildAssembly(CsvDataContextDriverProperties props, AssemblyName assemblyName, ref string nameSpace, ref string typeName)
        {
            Logger.LogEnabled = props.DebugInfo;
            Logger.Log("Build started: " + props.Files);
            var sw = Stopwatch.StartNew();
            var sw2 = Stopwatch.StartNew();

            CsvDatabase db = CsvDataModelGenerator.CreateModel(props);

            Logger.Log("Model created. ({0} ms)", sw2.ElapsedMilliseconds); sw2.Restart();

            string code = CsvCSharpCodeGenerator.GenerateCode(db, ref nameSpace, ref typeName, props);

            Logger.Log("Code generated. ({0} ms)", sw2.ElapsedMilliseconds); sw2.Restart();

            string[] compileErrors = BuildAssembly(code, assemblyName);

            Logger.Log("Assembly compiled. ({0} ms)", sw2.ElapsedMilliseconds); sw2.Restart();

            List<ExplorerItem> schema = GetSchema(db, props);

            Logger.Log("Schema tree created. ({0} ms)", sw2.ElapsedMilliseconds); sw2.Restart();

            bool anyCompileError = compileErrors != null && compileErrors.Any();
            if (anyCompileError || props.DebugInfo)
            {
                schema.Insert(0, new ExplorerItem("Context Source Code", ExplorerItemKind.Schema, ExplorerIcon.Schema)
                {
                    ToolTipText = "Data Context source code. Drag&drop to text window.",
                    DragText = code,
                });
            }
            if (anyCompileError)
            {
                Logger.Log("Errors: {0}", compileErrors.Length);
                schema.Insert(0, new ExplorerItem("Context Compile ERROR", ExplorerItemKind.Schema, ExplorerIcon.Box)
                {
                    ToolTipText = "Data context compile failed. Drag&Drop error messages to text window to see them.",
                    DragText = string.Join("\n", compileErrors),
                });
            }
            if (db.Tables.Count == 0)
            {
                schema.Insert(0, new ExplorerItem("No files found.", ExplorerItemKind.Schema, ExplorerIcon.Box));
            }

            Logger.Log("Tables: {0} Columns: {1} Relations: {2}", db.Tables.Count(), db.Tables.Sum(t => t.Columns.Count()), db.Tables.Sum(t => t.Relations.Count()) );
            Logger.Log("Build finished. ({0} ms)", sw.ElapsedMilliseconds);

            Logger.Log( string.Join("\n", db.Tables.SelectMany(t => t.Relations.Select( r => r.CodeName) )));

            return schema;
        }

        /// <summary>
        /// Compile generated code into assembly
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string[] BuildAssembly(string code, AssemblyName name) 
        {
            CompilerResults results;
            using (var codeProvider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } })) {
                var options = new CompilerParameters(new string[]
                    {
                        typeof(SchemaBuilder).Assembly.Location,
                        typeof(CsvReader).Assembly.Location,
                        "System.dll", "System.Core.dll", "System.Xml.dll", "System.Data.Services.Client.dll",
                    }){
                    IncludeDebugInformation = true, 
                    OutputAssembly = name.CodeBase,
                    CompilerOptions = @"/doc:""" + Path.ChangeExtension( name.CodeBase, "xml") + @"""",
                };
                results = codeProvider.CompileAssemblyFromSource(options, code);
            }

            if (!results.Errors.HasErrors)
                return null;

            return results.Errors.OfType<CompilerError>().Where(e => !e.IsWarning).Select(e => string.Format("{0},{1}: {2}", e.Line, e.Column, e.ErrorText)).ToArray();

        }

        /// <summary>
        /// Get LINQPad Schema from CSV data model
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private static List<ExplorerItem> GetSchema(CsvDatabase db, CsvDataContextDriverProperties props)
        {
            var schema = (
                from table in db.Tables ?? Enumerable.Empty<CsvTable>()
                select new ExplorerItem(table.DisplayName, ExplorerItemKind.QueryableObject, ExplorerIcon.Table) {
                    DragText = table.CodeName,
                    Children = (
                        from c in table.Columns ?? Enumerable.Empty<CsvColumn>()
                        select new ExplorerItem(c.DisplayName, ExplorerItemKind.Property, ExplorerIcon.Column) 
                        {
                            DragText = c.CodeName,
                        }
                    ).Concat(
                        from r in table.Relations
                        select new ExplorerItem(r.DisplayName, ExplorerItemKind.CollectionLink, ExplorerIcon.ManyToMany)
                        {
                            DragText = props.RelationsAsMethods ? "Get"+r.CodeName+"()" : r.CodeName,
                            //TODO HyperlinkTarget =                             
                        }
                    ).ToList(),
                }
            ).ToList();
            return schema;
        }
    }
}
