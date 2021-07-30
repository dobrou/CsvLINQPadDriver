using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using CsvLINQPadDriver.Extensions;

using LINQPad.Extensibility.DataContext;

// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver
{
    public class CsvDataContextDriverProperties : ICsvDataContextDriverProperties
    {
        private readonly IConnectionInfo _connectionInfo;
        private readonly XElement _driverData;

        public CsvDataContextDriverProperties(IConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
            _driverData = connectionInfo.DriverData;
        }

        public bool IsProduction
        {
            get => _connectionInfo.IsProduction;
            set => _connectionInfo.IsProduction = value;
        }

        public bool Persist
        {
            get => _connectionInfo.Persist;
            set => _connectionInfo.Persist = value;
        }

        public string Files
        {
            get => GetValue(string.Empty)!;
            set => SetValue(value);
        }

        public FileType FileType
        {
            get => GetValue(FileType.CSV);
            set => SetValue(value);
        }

        public FilesOrderBy FilesOrderBy
        {
            get => GetValue(FilesOrderBy.None);
            set => SetValue(value);
        }

        public NoBomEncoding NoBomEncoding
        {
            get => GetValue(NoBomEncoding.UTF8);
            set => SetValue(value);
        }

        public bool IgnoreBadData
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public bool AutoDetectEncoding
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool AllowComments
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public string CommentChars
        {
            get => GetValue(string.Empty)!;
            set => SetValue(value);
        }

        public char? CommentChar =>
            string.IsNullOrWhiteSpace(CommentChars)
                ? null
                : CommentChars.TrimStart().First();

        public IEnumerable<string> ParsedFiles =>
            Files.GetFilesOnly();

        public string CsvSeparator
        {
            get => GetValue(string.Empty)!;
            set => SetValue(value);
        }

        public char? CsvSeparatorChar
        {
            get
            {
                var csvSeparator = CsvSeparator;

                if (string.IsNullOrEmpty(csvSeparator))
                {
                    return null;
                }

                try
                {
                    return Regex.Unescape(csvSeparator).FirstOrDefault();
                }
                catch (Exception exception) when (exception.CanBeHandled())
                {
                    var fallbackCsvSeparator = csvSeparator.First();

                    CsvDataContextDriver.WriteToLog($"Falling back to CSV separator {fallbackCsvSeparator}", exception);

                    return fallbackCsvSeparator;
                }
            }
        }

        public bool IgnoreBlankLines
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public WhitespaceTrimOptions WhitespaceTrimOptions
        {
            get => GetValue(WhitespaceTrimOptions.None);
            set => SetValue(value);
        }

        public bool UseCsvHelperSeparatorAutoDetection
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public bool UseRecordType
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool UseSingleClassForSameFiles
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool ShowSameFilesNonGrouped
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public StringComparison StringComparison
        {
            get => GetValue(StringComparison.Ordinal);
            set => SetValue(value);
        }

        public bool DetectRelations
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool HideRelationsFromDump
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool DebugInfo
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public bool ValidateFilePaths
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool IgnoreInvalidFiles
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public bool IsStringInternEnabled
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool UseStringComparerForStringIntern
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public bool IsCacheEnabled
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        private T GetValue<T>(Func<string?, T> convert, T defaultValue, [CallerMemberName] string callerMemberName = "") =>
            convert(_driverData.Element(callerMemberName)?.Value) ?? defaultValue;

        private bool GetValue(bool defaultValue, [CallerMemberName] string callerMemberName = "") =>
            GetValue(v => v.ToBool(), defaultValue, callerMemberName)!.Value;

        private string? GetValue(string defaultValue, [CallerMemberName] string callerMemberName = "") =>
            GetValue(v => v, defaultValue, callerMemberName);

        private T GetValue<T>(T defaultValue, [CallerMemberName] string callerMemberName = "")
#if NETCOREAPP
            where T: Enum =>
            (T)
#else
            where T : struct =>
#endif
            GetValue(v =>
#if NETCOREAPP
                Enum.TryParse(typeof(T), v, out var val)
#else
                Enum.TryParse(v, out T val)
#endif
                    ? val
                    : defaultValue,
                defaultValue, callerMemberName)!;

        private void SetValue<T>(T value, [CallerMemberName] string callerMemberName = "") =>
            _driverData.SetElementValue(callerMemberName, value);
    }
}