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
    public class CsvDataContextDriverProperties :
        ICsvDataContextDriverProperties,
        IEquatable<CsvDataContextDriverProperties>
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
            Files.GetFiles();

        public string CsvSeparator
        {
            get => GetValue(string.Empty)!;
            set => SetValue(value);
        }

        public string? SafeCsvSeparator
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
                    return Regex.Unescape(csvSeparator);
                }
                catch (Exception exception) when (exception.CanBeHandled())
                {
                    $"Falling back to CSV separator '{csvSeparator}'".WriteToLog(DebugInfo, exception);

                    return csvSeparator;
                }
            }
        }

        public bool IgnoreBlankLines
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public bool AddHeader
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public HeaderDetection HeaderDetection
        {
            get => GetValue(HeaderDetection.AllLettersNumbersPunctuation);
            set => SetValue(value);
        }

        public HeaderFormat HeaderFormat
        {
            get => GetValue(HeaderFormat.c1);
            set => SetValue(value);
        }

        public bool TrimSpaces
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public WhitespaceTrimOptions WhitespaceTrimOptions
        {
            get => GetValue(WhitespaceTrimOptions.All);
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

        public bool DoNotLockFiles
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

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CsvDataContextDriverProperties) obj);
        }

        public bool Equals(CsvDataContextDriverProperties? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return PropertiesEqual().All(static _ => _);

            IEnumerable<bool> PropertiesEqual()
            {
                yield return GetFiles(ParsedFiles).SequenceEqual(GetFiles(other.ParsedFiles), FileExtensions.FileNameComparer);

                yield return FilesOrderBy == other.FilesOrderBy;
                yield return NoBomEncoding == other.NoBomEncoding;
                yield return AutoDetectEncoding == other.AutoDetectEncoding;
                yield return IgnoreInvalidFiles == other.IgnoreInvalidFiles;

                yield return UseCsvHelperSeparatorAutoDetection == other.UseCsvHelperSeparatorAutoDetection;
                if (!UseCsvHelperSeparatorAutoDetection && !other.UseCsvHelperSeparatorAutoDetection)
                {
                    yield return SafeCsvSeparator == other.SafeCsvSeparator;
                }

                yield return IgnoreBadData == other.IgnoreBadData;
                yield return IgnoreBlankLines == other.IgnoreBlankLines;

                yield return TrimSpaces == other.TrimSpaces;
                if (TrimSpaces && other.TrimSpaces)
                {
                    yield return WhitespaceTrimOptions == other.WhitespaceTrimOptions;
                }

                yield return AllowComments == other.AllowComments;
                if (AllowComments && other.AllowComments)
                {
                    yield return CommentChar == other.CommentChar;
                }

                yield return AddHeader == other.AddHeader;
                if (AddHeader && other.AddHeader)
                {
                    yield return HeaderDetection == other.HeaderDetection &&
                                 HeaderFormat == other.HeaderFormat;
                }

                yield return IsCacheEnabled == other.IsCacheEnabled;

                yield return IsStringInternEnabled == other.IsStringInternEnabled;
                if (IsStringInternEnabled && other.IsStringInternEnabled)
                {
                    yield return UseStringComparerForStringIntern == other.UseStringComparerForStringIntern;
                }

                yield return UseRecordType == other.UseRecordType;
                yield return UseSingleClassForSameFiles == other.UseSingleClassForSameFiles;
                yield return StringComparison == other.StringComparison;

                yield return DetectRelations == other.DetectRelations;

                IEnumerable<string> GetFiles(IEnumerable<string> files) =>
                    FilesOrderBy == FilesOrderBy.None
                        ? files
                        : files.OrderBy(static _ => _, FileExtensions.FileNameComparer);
            }
        }

        public override int GetHashCode() =>
            HashCode.Combine(_connectionInfo, _driverData);

        private T GetValue<T>(Func<string?, T> convert, T defaultValue, [CallerMemberName] string callerMemberName = "") =>
            convert(_driverData.Element(callerMemberName)?.Value) ?? defaultValue;

        private bool GetValue(bool defaultValue, [CallerMemberName] string callerMemberName = "") =>
            GetValue(static v => v.ToBool(), defaultValue, callerMemberName)!.Value;

        private string? GetValue(string defaultValue, [CallerMemberName] string callerMemberName = "") =>
            GetValue(static v => v, defaultValue, callerMemberName);

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