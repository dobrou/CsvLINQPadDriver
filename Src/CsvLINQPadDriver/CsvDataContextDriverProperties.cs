using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using CsvLINQPadDriver.Helpers;

using LINQPad.Extensibility.DataContext;

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

        public IEnumerable<string> ParsedFiles =>
            Regex.Split(Files, @"[\r\n]+")
                .GetFilesOnly()
                .Distinct(StringComparer.InvariantCultureIgnoreCase);

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
                catch (Exception exception)
                {
                    var fallbackCsvSeparator = csvSeparator.First();

                    CsvDataContextDriver.WriteToLog($"Falling back to CSV separator {fallbackCsvSeparator}", exception);

                    return fallbackCsvSeparator;
                }
            }
        }

        public bool UseSingleClassForSameFiles
        {
            get => GetValue(true);
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

        private void SetValue<T>(T value, [CallerMemberName] string callerMemberName = "") =>
            _driverData.SetElementValue(callerMemberName, value);
    }
}