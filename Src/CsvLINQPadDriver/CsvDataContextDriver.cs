using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.Helpers;
using LINQPad;
using LINQPad.Extensibility.DataContext;

namespace CsvLINQPadDriver
{
    public class CsvDataContextDriver : DynamicDataContextDriver
    {
        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return FileUtils.GetLongestCommonPrefixPath(new CsvDataContextDriverProperties(cxInfo).Files.Split('\n').Select(f => f.Trim()).ToArray());
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions)
        {
            var properties = new CsvDataContextDriverProperties(cxInfo);
            if (dialogOptions.IsNewConnection)
            {
                properties.Files = string.Join(Environment.NewLine,
                    "# Drag&drop (use Ctrl to add files)",
                    "# Type file paths, or directory paths with pattern like *.csv or **.csv (** will recurse subdirectories)",
                    "# Press Ctrl+Shift+V to insert from clipboard and proceed",
                    @"c:\*.csv");
            }

            bool? result = new ConnectionDialog(properties).ShowDialog();
            if (result == true)
            {
                cxInfo.DisplayName = GetConnectionDescription(cxInfo);
                return true;
            }
            return false;
        }

        public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {            
            return CsvRowMemberProvider.GetCsvRowMemberProvider(objectToWrite) ?? base.GetCustomDisplayMemberProvider(objectToWrite);
        }

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            return new[] { "CsvHelper.dll", "CsvLINQPadDriver.dll" };
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            return new[] {typeof (StringExtensions).Namespace};
        }

        public override string Name => "CSV Context Driver";

        public override string Author => "Martin Dobroucký (dobrou@gmail.com), Ivan Ivon (ivan.ivon@gmail.com)";

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            return SchemaBuilder.GetSchemaAndBuildAssembly(
                new CsvDataContextDriverProperties(cxInfo),
                assemblyToBuild,
                ref nameSpace,
                ref typeName);
        }
    }
}
