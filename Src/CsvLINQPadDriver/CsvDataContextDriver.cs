using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Humanizer;

using LINQPad;
using LINQPad.Extensibility.DataContext;

using CsvLINQPadDriver.DataDisplay;
using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CsvDataContextDriver : DynamicDataContextDriver
    {
        public override string Name =>
            "CSV Context Driver";

        public override Version Version =>
            Assembly.GetExecutingAssembly().GetName().Version!;

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public override string Author =>
            "Martin Dobroucký (dobrou@gmail.com), Ivan Ivon (ivan.ivon@gmail.com)";

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            var parsedFiles = FileUtils.EnumFiles(new CsvDataContextDriverProperties(cxInfo).ParsedFiles).ToImmutableList();
            var parsedFilesCount = parsedFiles.Count;
            var dateTime = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            var totalFilesSize = parsedFiles.GetHumanizedFileSize();

            return $"{FileUtils.GetLongestCommonPrefixPath(parsedFiles)}{GetFilesCountString()}";

            string GetFilesCountString() =>
                parsedFilesCount switch
                {
                    0 => $"({dateTime}, no files)",
                    1 => $" ({totalFilesSize}, {dateTime})",
                    _ => $" ({dateTime}, {"file".ToQuantity(parsedFilesCount)}, {totalFilesSize})"
                };
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, ConnectionDialogOptions connectionDialogOptions)
        {
            var properties = new CsvDataContextDriverProperties(cxInfo);

            if (connectionDialogOptions.IsNewConnection)
            {
                properties.Files = string.Join(Environment.NewLine,
                    "# Drag&drop (use Ctrl to add files)",
                    "# Type file paths, or directory paths with pattern like *.csv or **.csv (** will recurse subdirectories)",
                    "# Press Ctrl+Shift+V to insert from clipboard and proceed",
                    $"{FileUtils.GetDefaultDrive()}*.csv");
            }

            if (new ConnectionDialog(properties).ShowDialog() != true)
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
            yield return typeof(Helpers.StringExtensions).Namespace!;
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
