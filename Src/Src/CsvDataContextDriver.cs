using System;
using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.Helpers;
using LINQPad;
using LINQPad.Extensibility.DataContext;
using System.Collections.Generic;
using System.Reflection;

namespace CsvLINQPadDriver
{
    public class CsvDataContextDriver : DynamicDataContextDriver
    {
        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return FileUtils.GetLongestCommonPrefixPath(new CsvDataContextDriverProperties(cxInfo).Files.Split('\n'));
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            var properties = new CsvDataContextDriverProperties(cxInfo);
            if (isNewConnection)
            {
                properties.Files = "#Drag&Drop or type file paths, or directory paths with pattern like *.csv or **.csv (** will recurse subdirectory)\nc:\\*.csv";
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

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            return new[] { "CsvHelper.dll", "CsvLINQPadDriver.dll" };
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            return new[] {typeof (StringExtensions).Namespace};
        }

        public override string Name {
            get { return "CSV Context Driver"; }
        }

        public override string Author {
            get { return "Martin Dobroucký (dobrou@gmail.com)"; }
        }

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
