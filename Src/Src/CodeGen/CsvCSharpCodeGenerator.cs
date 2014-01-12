using CsvLINQPadDriver.DataModel;
using System;
using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{

    /// <summary>
    /// Generates data context and classes source code from data model.
    /// </summary>
    internal class CsvCSharpCodeGenerator
    {
        public static string GenerateCode(CsvDatabase db, ref string nameSpace, ref string typeName, CsvDataContextDriverProperties props)
        {
            return new CsvCSharpCodeGenerator(nameSpace, typeName, props).GenerateSrcFile(db);
        }

        private string contextNameSpace;
        private string contextTypeName;
        private CsvDataContextDriverProperties properties;
      
        public CsvCSharpCodeGenerator(string contextNameSpace, string contextTypeName, CsvDataContextDriverProperties properties)
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
        public " + typeof(CsvTableBase<,>).GetCodeTypeClassName(table.GetCodeRowClassName(), contextTypeName) + @" " + table.CodeName + @" { get; private set; }"
) + @"       

        public " + contextTypeName + @"()
        {
            //Init tables data " 
+ string.Join("", from table in db.Tables select @"
            this." + table.CodeName + @" = new " + typeof(CsvTableBase<,>).GetCodeTypeClassName(table.GetCodeRowClassName(), contextTypeName) + @"( this,
                '" + table.CsvSeparator + @"', @""" + table.FilePath + @""",
                new " + typeof(CsvColumnInfoList<>).GetCodeTypeClassName(table.GetCodeRowClassName()) + @"() { "
    + string.Join("", from c in table.Columns select @"
                    { " + c.CsvColumnIndex + @", x => x." + c.CodeName + @" }, ") + @"
                }
            ); "
) + @"  
        }
    }//context class

    //Data types "
+ string.Join("", from table in db.Tables select 
        GenerateTableRowDataTypeClass(table, db, properties.RelationsAsMethods)
) + @"       
}//namespace
";
            return src;
        }

        internal string GenerateTableRowDataTypeClass(CsvTable table, CsvDatabase db, bool relationsAsMethods)
        {
            var src = @"
    public class " + table.GetCodeRowClassName() + @" : " + typeof(CsvRowBase<>).GetCodeTypeClassName() + @"<" + contextTypeName + @">
    {"
+ string.Join("", from c in table.Columns select @"
        public string " + c.CodeName + @" { get; set; } "
) + string.Join("", from rel in table.Relations select @"
        /// <summary>" + System.Security.SecurityElement.Escape(rel.DisplayName) + @"</summary>
        public IEnumerable<" + rel.TargetTable.GetCodeRowClassName() + @"> " + rel.CodeName + (relationsAsMethods ? "() {" : " { get") + @" { return base.Context." + rel.TargetTable.CodeName + @".WhereIndexed(""" + rel.TargetColumn.CodeName + @""", this." + rel.SourceColumn.CodeName + @"); } }"
) + @"
    } "
;
            return src;
        }

    }

    internal static class CsvCSharpCodeGeneratorExtensions
    {
        static internal string GetCodeRowClassName(this CsvTable table)
        {
            return "T" + table.CodeName + "Row";
        }
        static internal string GetCodeTypeClassName(this Type type, params string[] genericParameters)
        {
            return type.FullName.Split('`')[0] + (genericParameters.Length == 0 ? "" : "<" + string.Join(",", genericParameters) + ">");
        }
    }

}
