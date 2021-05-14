using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public override string Author =>
            // ReSharper disable StringLiteralTypo
            "Martin Dobroucký (dobrou@gmail.com), Ivan Ivon (ivan.ivon@gmail.com)";
            // ReSharper restore StringLiteralTypo

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            var parsedFiles = new CsvDataContextDriverProperties(cxInfo).ParsedFiles.EnumFiles().ToImmutableList();
            var parsedFilesCount = parsedFiles.Count;
            var dateTime = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            var filesAndTotalSize = $"{"file".ToQuantity(parsedFilesCount)} {parsedFiles.GetHumanizedFileSize()}";

            return $"{parsedFiles.GetLongestCommonPrefixPath()}{GetFilesCountString()}";

            string GetFilesCountString() =>
                parsedFilesCount switch
                {
                    0 => $"({dateTime}, no files)",
                    _ => $" ({dateTime}, {filesAndTotalSize})"
                };
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, ConnectionDialogOptions connectionDialogOptions)
        {
            var properties = new CsvDataContextDriverProperties(cxInfo);

            if (connectionDialogOptions.IsNewConnection)
            {
                properties.Files = string.Join(Environment.NewLine,
                    "# Drag&drop here. Use Ctrl to add files.",
                    "# Type one file/folder per line. Wildcards ? and * are supported; **.csv searches in folder and its sub-folders.",
                    "# Press Ctrl+Shift+V to clear, paste from clipboard and proceed.",
                    "# Press Ctrl+Shift+Alt+V to paste from clipboard and proceed.",
                    string.Empty, string.Empty);
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
