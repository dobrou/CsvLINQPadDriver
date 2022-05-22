using System;
using System.Collections.Generic;
using System.Reflection;

using Humanizer;

using LINQPad;
using LINQPad.Extensibility.DataContext;

using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.Extensions;

#if NETCOREAPP
using System.Collections.Immutable;
#else
using CsvLINQPadDriver.Bcl.Extensions;
#endif

namespace CsvLINQPadDriver
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CsvDataContextDriver : DynamicDataContextDriver
    {
        public override string Name =>
            "CSV Context Driver";

        public override Version Version =>
            Assembly.GetExecutingAssembly().GetName().Version!;

        public override string Author =>
            // ReSharper disable StringLiteralTypo
            "Martin Dobroucký (dobrou@gmail.com), Ivan Ivon (ivan.ivon@gmail.com)";
            // ReSharper restore StringLiteralTypo

        public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2) =>
            new CsvDataContextDriverProperties(c1).Equals(new CsvDataContextDriverProperties(c2));

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            var csvDataContextDriverProperties = new CsvDataContextDriverProperties(cxInfo);

            var parsedFiles = csvDataContextDriverProperties.ParsedFiles.EnumFiles().ToImmutableList();
            var parsedFilesCount = parsedFiles.Count;
            var dateTime = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            var filesAndTotalSize = $"{"file".ToQuantity(parsedFilesCount)} {parsedFiles.GetHumanizedFileSize(csvDataContextDriverProperties.DebugInfo)}";

            return $"{parsedFiles.GetLongestCommonPrefixPath()}{GetFilesCountString()}";

            string GetFilesCountString() =>
                parsedFilesCount switch
                {
                    0 => $"({dateTime}, no files)",
                    _ => $" ({dateTime}, {filesAndTotalSize})"
                };
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, ConnectionDialogOptions dialogOptions)
        {
            if (new ConnectionDialog(new CsvDataContextDriverProperties(cxInfo)).ShowDialog() != true)
            {
                return false;
            }

            cxInfo.DisplayName = GetConnectionDescription(cxInfo);

            return true;
        }

        public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite) =>
            CsvRowMemberProvider.GetCsvRowMemberProvider(objectToWrite) ??
            base.GetCustomDisplayMemberProvider(objectToWrite);

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            yield return typeof(Extensions.StringExtensions).Namespace!;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName) =>
            SchemaBuilder.GetSchemaAndBuildAssembly(
                new CsvDataContextDriverProperties(cxInfo),
                assemblyToBuild,
                ref nameSpace,
                ref typeName);

        internal static void WriteToLog(string additionalInfo, Exception? exception = null)
        {
            const string logFileName = nameof(CsvLINQPadDriver) + ".txt";

            if (exception is null)
            {
                WriteToLog(additionalInfo, logFileName);
            }
            else
            {
                WriteToLog(exception, logFileName, additionalInfo);
            }
        }
    }
}
