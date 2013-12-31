using System;
using System.Linq;
using CsvLINQPadDriver.DataModel;
using CsvLINQPadDriver.Helpers;

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
    //CSV Data Context
    public class " + contextTypeName + @" : " + GetClassName(typeof(CsvDataContextBase)) + @" 
    {
        //Tables instances " 
+ string.Join("", from table in db.Tables select @"
        public " + GetTableClassName(table) + @" " + table.CodeName + @" { get; private set; }"
) + @"       

        public " + contextTypeName + @"()
        {
            //Init tables data " 
+ string.Join("", from table in db.Tables select @"
            this." + table.CodeName + @" = new " + GetTableClassName(table) + @"( this, @""" + table.FilePath + @""", '" + table.CsvSeparator + @"'); "
) + @"  
        }
    }//context class

    //Data types "
+ string.Join("", from table in db.Tables select 
        GenerateTableRowDataTypeClass(table, db, properties.RelationsAsMethods)
        + GenerateTableClass(table, db)
        + GenerateTableRowDataTypeMappingClass(table, db)
) + @"       
}//namespace
";
            return src;
        }

        internal string GenerateTableClass(CsvTable table, CsvDatabase db)
        {
            var src = @"
    public class " + GetTableClassName(table) + @" : " + GetClassName(typeof(CsvTableBase<,>)) + @"<" + GetRowClassName(table) + @"," + contextTypeName + @">
    { 
        public " + GetTableClassName(table) + @"(" + contextTypeName + @" dataContext, string fileName, char csvSeparator)
        : base( dataContext, fileName, csvSeparator, new " + GetRowMappingClassName(table) + @"(dataContext)) 
        {}

        //Where and Indexes
" + string.Join("\n", from c in table.Columns select @"
        private ILookup< string, " + GetRowClassName(table) + @"> index" + c.CodeName + @" = null;
        public IEnumerable<" + GetRowClassName(table) + @"> Where" + c.CodeName + @"(params string[] values)
        { 
            CsvLINQPadDriver.Helpers.Logger.Log(""" + GetTableClassName(table) + @".Where" + c.CodeName + @"({0})"", string.Join("","", values)); 
            if( index" + c.CodeName + @" == null ) index" + c.CodeName + @" = this.ToLookup(x => x." + c.CodeName + @", StringComparer.Ordinal);
            var result = values.SelectMany( value => index" + c.CodeName + @"[value] ); 
            return values.Count() > 1 ? result.Distinct() : result;
        } "
) + @"       
    } "
;
            return src;
        }

        internal string GenerateTableRowDataTypeMappingClass(CsvTable table, CsvDatabase db)
        {
            var src = @"
    internal class " + GetRowMappingClassName(table) + @" : " + GetClassName(typeof(CsvRowMappingBase<,>)) + @"<" + GetRowClassName(table) + @"," + contextTypeName + @">
    {
        public " + GetRowMappingClassName(table) + @"(" + contextTypeName + @" dataContext) : base( dataContext ) {}

        public override void CreateMap()
        {
            CsvLINQPadDriver.Helpers.Logger.Log(""" + GetRowMappingClassName(table) + @".CreateMap"");
" + string.Join("", from c in table.Columns select @"
            Map( c => c." + c.CodeName + @" ).Index(" + c.CsvColumnIndex + @");" //.Name(c.CsvColumnName)
) + string.Join("", from rel in table.Relations select @"
            Map( c => c." + rel.CodeName + @").ConvertUsing( row => { 
                var sourceId = row.GetField(" + rel.SourceColumn.CsvColumnIndex + @" /*" + rel.SourceColumn.CodeName + @"*/); 
                return new " + GetClassName(typeof(LazyEnumerable<>)) + @"<" + GetRowClassName(rel.TargetTable) + @">( () => {
                    return this.dataContext." + rel.TargetTable.CodeName + @".Where" + rel.TargetColumn.CodeName + @"( sourceId );
                });
            });"
) + @"
        }        
    } "
;
            return src;
        }

        internal string GenerateTableRowDataTypeClass(CsvTable table, CsvDatabase db, bool relationsAsMethods)
        {
            var src = @"
    public class " + GetRowClassName(table) + @" : " + GetClassName(typeof(CsvRowBase)) + @"
    {
        //Columns "
+ string.Join("", from c in table.Columns select @"
        public string " + c.CodeName + @" { get; set; } "
) + @"       
        //Relations " 
+ string.Join("", from rel in table.Relations select @"
        " + (relationsAsMethods ? "internal" : "public") + @" IEnumerable<" + GetRowClassName(rel.TargetTable) + @"> " + rel.CodeName + @" { get; " + (relationsAsMethods ? "" : "internal") + @" set; }
        public IEnumerable<" + GetRowClassName(rel.TargetTable) + @"> Get" + rel.CodeName + @"() { return " + rel.CodeName + @"; } "
) + @"
    } "
;
            return src;
        }

        private string GetRowClassName(CsvTable table)
        {
            return "T" + table.CodeName;
        }
        private string GetRowMappingClassName(CsvTable table)
        {
            return "T" + table.CodeName + "Mapping";
        }
        private string GetTableClassName(CsvTable table)
        {
            return "T" + table.CodeName + "Table";
        }
        private string GetClassName(Type type)
        {
            return type.FullName.Split('`')[0];
        }

    }

}
