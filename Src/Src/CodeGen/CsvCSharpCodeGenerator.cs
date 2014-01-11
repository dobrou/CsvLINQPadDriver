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
    /// <summary>CSV Data Context</summary>
    public class " + contextTypeName + @" : " + typeof(CsvDataContextBase).GetCodeTypeClassName() + @" 
    {

        //Tables instances "
+ string.Join("", from table in db.Tables select @"
        /// <summary>File: "+ System.Security.SecurityElement.Escape(table.FilePath) +@"</summary>
        public " + table.GetCodeTableClassName() + @" " + table.CodeName + @" { get; private set; }"
) + @"       

        public " + contextTypeName + @"()
        {
            //Init tables data " 
+ string.Join("", from table in db.Tables select @"
            this." + table.CodeName + @" = new " + table.GetCodeTableClassName() + @"( this, @""" + table.FilePath + @""", '" + table.CsvSeparator + @"'); "
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
    public class " + table.GetCodeTableClassName() + @" : " + typeof(CsvTableBase<,>).GetCodeTypeClassName() + @"<" + table.GetCodeRowClassName() + @"," + contextTypeName + @">
    { 
        public " + table.GetCodeTableClassName() + @"(" + contextTypeName + @" dataContext, string fileName, char csvSeparator)
        : base( dataContext, fileName, csvSeparator, new " + table.GetCodeRowMappingClassName() + @"(dataContext)) 
        {}

        //Where and Indexes
" + string.Join("\n", from c in table.Columns select @"
        private ILookup< string, " + table.GetCodeRowClassName() + @"> index" + c.CodeName + @" = null;
        public IEnumerable<" + table.GetCodeRowClassName() + @"> Where" + c.CodeName + @"(params string[] values)
        { 
            CsvLINQPadDriver.Helpers.Logger.Log(""" + table.GetCodeTableClassName() + @".Where" + c.CodeName + @"({0})"", string.Join("","", values)); 
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
    internal class " + table.GetCodeRowMappingClassName() + @" : " + typeof(CsvRowMappingBase<,>).GetCodeTypeClassName() + @"<" + table.GetCodeRowClassName() + @"," + contextTypeName + @">
    {
        public " + table.GetCodeRowMappingClassName() + @"(" + contextTypeName + @" dataContext) : base( dataContext ) {}

        public override void CreateMap()
        {
            CsvLINQPadDriver.Helpers.Logger.Log(""" + table.GetCodeRowMappingClassName() + @".CreateMap"");
" + string.Join("", from c in table.Columns select @"
            Map( c => c." + c.CodeName + @" ).Index(" + c.CsvColumnIndex + @");" //.Name(c.CsvColumnName)
) + string.Join("", from rel in table.Relations select @"
            Map( c => c." + rel.CodeName + @").ConvertUsing( row => { 
                var sourceId = row.GetField(" + rel.SourceColumn.CsvColumnIndex + @" /*" + rel.SourceColumn.CodeName + @"*/); 
                return new " + typeof(LazyEnumerable<>).GetCodeTypeClassName() + @"<" + rel.TargetTable.GetCodeRowClassName() + @">( () => {
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
    public class " + table.GetCodeRowClassName() + @" : " + typeof(CsvRowBase).GetCodeTypeClassName() + @"
    {
        //Columns "
+ string.Join("", from c in table.Columns select @"
        public string " + c.CodeName + @" { get; set; } "
) + @"       
        //Relations " 
+ string.Join("", from rel in table.Relations select @"
        /// <summary>" + System.Security.SecurityElement.Escape(rel.DisplayName) + @"</summary>
        " + (relationsAsMethods ? "internal" : "public") + @" IEnumerable<" + rel.TargetTable.GetCodeRowClassName() + @"> " + rel.CodeName + @" { get; " + (relationsAsMethods ? "" : "internal") + @" set; }
        /// <summary>" + System.Security.SecurityElement.Escape(rel.DisplayName) + @"</summary>
        public IEnumerable<" + rel.TargetTable.GetCodeRowClassName() + @"> Get" + rel.CodeName + @"() { return " + rel.CodeName + @"; } "
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
        static internal string GetCodeRowMappingClassName(this CsvTable table)
        {            
            return "T" + table.CodeName + "Mapping";
        }
        static internal string GetCodeTableClassName(this CsvTable table)
        {
            return "T" + table.CodeName + "Table";
        }
        static internal string GetCodeTypeClassName(this Type type)
        {
            return type.FullName.Split('`')[0];
        }
    }

}
