using System.IO;
using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.DataModel;
using CsvLINQPadDriver.Helpers;
using System;
using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{

    /// <summary>
    /// Generates data context and classes source code from data model.
    /// </summary>
    public class CsvCSharpCodeGenerator
    {
        public const string DefaultContextTypeName = "CsvDataContext";

        public static string GenerateCode(CsvDatabase db, ref string nameSpace, ref string typeName, ICsvDataContextDriverProperties props)
        {
            typeName = DefaultContextTypeName;
            return new CsvCSharpCodeGenerator(nameSpace, DefaultContextTypeName, props).GenerateSrcFile(db);
        }

        private string contextNameSpace;
        private string contextTypeName;
        private ICsvDataContextDriverProperties properties;
      
        public CsvCSharpCodeGenerator(string contextNameSpace, string contextTypeName, ICsvDataContextDriverProperties properties)
        {
            this.contextNameSpace = contextNameSpace;
            this.contextTypeName = contextTypeName;
            this.properties = properties;
        }
        
        public string GenerateSrcFile(CsvDatabase db)
        {
            var src = 
@"using System;
using System.Linq;
using System.Collections.Generic;

namespace " + contextNameSpace + @"
{
    /// <summary>CSV Data Context</summary>
    public class " + contextTypeName + @" : " + typeof(CsvDataContextBase).GetCodeTypeClassName() + @" 
    { "
+ string.Join("", from table in db.Tables select @"
        /// <summary>File: "+ System.Security.SecurityElement.Escape(table.FilePath) +@"</summary>
        public " + typeof(CsvTableBase<>).GetCodeTypeClassName(table.GetCodeRowClassName()) + @" " + table.CodeName + @" { get; private set; }"
) + @"       

        public " + contextTypeName + @"()
        {
            //Init tables data " 
+ string.Join("", from table in db.Tables select @"
            this." + table.CodeName + @" = " + typeof(CsvTableFactory).GetCodeTypeClassName() + @".CreateTable<" + table.GetCodeRowClassName() + @">(
                " + (properties.IsStringInternEnabled ? "true" : "false") + @", 
                " + (properties.IsCacheEnabled ? "true" : "false") + @", 
                " + table.CsvSeparator.GetCodeCharEscaped() + @", 
                " + table.FilePath.GetCodeStringEscaped() + @",
                new " + typeof(CsvColumnInfoList<>).GetCodeTypeClassName(table.GetCodeRowClassName()) + @"() { "
    + string.Join("", from c in table.Columns select @"
                    { " + c.CsvColumnIndex + @", x => x." + c.CodeName + @" }, ") + @"
                },
                r => { r.__context = this; }
            ); "
) + @"  
        }
    }//context class

    //Data types "
+ string.Join("", from table in db.Tables select 
        GenerateTableRowDataTypeClass(table, db, properties.HideRelationsFromDump)
) + @"       
}//namespace
";
            return src;
        }

        internal string GenerateTableRowDataTypeClass(CsvTable table, CsvDatabase db, bool hideRelationsFromDump)
        {
            var src = @"
    public class " + table.GetCodeRowClassName() + @" : " + typeof(CsvRowBase).GetCodeTypeClassName() + @"
    {
        internal " + contextTypeName + " __context;"
+ string.Join("", from c in table.Columns select @"
        public string " + c.CodeName + @" { get; set; } "
) + string.Join("", from rel in table.Relations select @"
        /// <summary>" + System.Security.SecurityElement.Escape(rel.DisplayName) + @"</summary> " + (hideRelationsFromDump ? @"
        [" + typeof(HideFromDumpAttribute).GetCodeTypeClassName() + "]" : "") + @"
        public IEnumerable<" + rel.TargetTable.GetCodeRowClassName() + @"> " + rel.CodeName + @" { get {
            return this.__context." + rel.TargetTable.CodeName + @".WhereIndexed( tr => tr." + rel.TargetColumn.CodeName + @" , " + rel.TargetColumn.CodeName.GetCodeStringEscaped() + @", this." + rel.SourceColumn.CodeName + @");
        } } "
) + @"
    } "
;
            return src;
        }

    }

    /// <summary>
    /// Methods usefull for CSharp source code generator
    /// </summary>
    internal static class CsvCSharpCodeGeneratorExtensions
    {
        static public string GetCodeRowClassName(this CsvTable table)
        {
            return "T" + table.CodeName;
        }
        static internal string GetCodeTypeClassName(this Type type, params string[] genericParameters)
        {
            return type.FullName.Split('`')[0] + (genericParameters.Length == 0 ? "" : "<" + string.Join(",", genericParameters) + ">");
        }

        /// <summary>
        /// Transorm string into form suitable to be pasted into source code.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetCodeStringEscaped(this string input)
        {
            using (var sw = new StringWriter())
            using (var codeProvider = new Microsoft.CSharp.CSharpCodeProvider())
            {
                codeProvider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(input), sw, null);
                return sw.ToString();
            }
        }

        public static string GetCodeCharEscaped(this char input)
        {
            using (var sw = new StringWriter())
            using (var codeProvider = new Microsoft.CSharp.CSharpCodeProvider())
            {
                codeProvider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(input), sw, null);
                return sw.ToString();
            }
        }

    }

}
