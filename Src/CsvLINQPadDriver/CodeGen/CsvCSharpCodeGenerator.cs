﻿using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Security;
using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.DataModel;
using CsvLINQPadDriver.Helpers;
using Microsoft.CSharp;

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
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));

            typeName = DefaultContextTypeName;
            return new CsvCSharpCodeGenerator(nameSpace, DefaultContextTypeName, props).GenerateSrcFile(db);
        }

        private readonly string contextNameSpace;
        private readonly string contextTypeName;
        private readonly ICsvDataContextDriverProperties properties;

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
        /// <summary>File: "+ SecurityElement.Escape(table.FilePath) +@"</summary>
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
                r => { "
    + string.Join("", from r in table.Relations select @"
                    r." + r.CodeName + @" = new " + typeof(LazyEnumerable<>).GetCodeTypeClassName(r.TargetTable.GetCodeRowClassName()) + @"( () => " + r.TargetTable.CodeName + @".WhereIndexed( tr => tr." + r.TargetColumn.CodeName + @" , " + r.TargetColumn.CodeName.GetCodeStringEscaped() + @", r." + r.SourceColumn.CodeName + @") );") + @"
                }
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
    {"
+ string.Join("", from c in table.Columns select @"
        public string " + c.CodeName + @" { get; set; } "
) + string.Join("", from rel in table.Relations select @"
        /// <summary>" + SecurityElement.Escape(rel.DisplayName) + @"</summary> " + (hideRelationsFromDump ? @"
        [" + typeof(HideFromDumpAttribute).GetCodeTypeClassName() + "]" : "") + @"
        public IEnumerable<" + rel.TargetTable.GetCodeRowClassName() + @"> " + rel.CodeName + @" { get; set; } "
) + @"
    } "
;
            return src;
        }

    }

    /// <summary>
    /// Methods useful for CSharp source code generator
    /// </summary>
    internal static class CsvCSharpCodeGeneratorExtensions
    {
        public static string GetCodeRowClassName(this CsvTable table)
        {
            return "T" + table.CodeName;
        }
        internal static string GetCodeTypeClassName(this Type type, params string[] genericParameters)
        {
            return type!.FullName!.Split('`')[0] + (genericParameters.Length == 0 ? "" : "<" + string.Join(",", genericParameters) + ">");
        }

        /// <summary>
        /// Transform string into form suitable to be pasted into source code.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetCodeStringEscaped(this string input)
        {
            using var sw = new StringWriter();
            using var codeProvider = new CSharpCodeProvider();

            codeProvider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), sw, null);
            return sw.ToString();
        }

        public static string GetCodeCharEscaped(this char input)
        {
            using var sw = new StringWriter();
            using var codeProvider = new CSharpCodeProvider();

            codeProvider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), sw, null);
            return sw.ToString();
        }
    }
}
