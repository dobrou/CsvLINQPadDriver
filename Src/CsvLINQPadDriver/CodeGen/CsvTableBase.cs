using System;
using System.Collections;
using System.Collections.Generic;

using CsvLINQPadDriver.Extensions;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvTableBase
    {
        public static readonly StringComparer StringComparer = StringComparer.Ordinal;

        internal bool IsStringInternEnabled { get; }

        protected CsvTableBase(bool isStringInternEnabled) =>
            IsStringInternEnabled = isStringInternEnabled;
    }

    public abstract class CsvTableBase<TRow> : CsvTableBase, IEnumerable<TRow>
        where TRow : ICsvRowBase, new()
    {
        private static CsvRowMappingBase<TRow>? _cachedCsvRowMappingBase;

        private readonly char? _csvSeparator;
        private readonly NoBomEncoding _noBomEncoding;
        private readonly StringComparer? _internStringComparer;
        private readonly bool _allowComments;
        private readonly char? _commentChar;
        private readonly bool _ignoreBadData;
        private readonly bool _autoDetectEncoding;
        private readonly bool _ignoreBlankLines;
        private readonly bool _doNotLockFiles;
        private readonly WhitespaceTrimOptions _whitespaceTrimOptions;

        protected readonly string FilePath;

        protected CsvTableBase(
            bool isStringInternEnabled,
            StringComparer? internStringComparer,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            bool doNotLockFiles,
            WhitespaceTrimOptions whitespaceTrimOptions,
            string filePath,
            IEnumerable<CsvColumnInfo> propertiesInfo,
            Action<TRow> relationsInit)
            : base(isStringInternEnabled)
        {
            _internStringComparer = internStringComparer;
            _csvSeparator = csvSeparator;
            _noBomEncoding = noBomEncoding;
            _allowComments = allowComments;
            _commentChar = commentChar;
            _ignoreBadData = ignoreBadData;
            _autoDetectEncoding = autoDetectEncoding;
            _ignoreBlankLines = ignoreBlankLines;
            _doNotLockFiles = doNotLockFiles;
            _whitespaceTrimOptions = whitespaceTrimOptions;

            FilePath = filePath;

            _cachedCsvRowMappingBase ??= new CsvRowMappingBase<TRow>(propertiesInfo, relationsInit);
        }

        protected IEnumerable<TRow> ReadData() =>
            FilePath.CsvReadRows(
                _csvSeparator,
                IsStringInternEnabled,
                _internStringComparer,
                _noBomEncoding,
                _allowComments,
                _commentChar,
                _ignoreBadData,
                _autoDetectEncoding,
                _ignoreBlankLines,
                _doNotLockFiles,
                _whitespaceTrimOptions,
                _cachedCsvRowMappingBase!);

        // ReSharper disable once UnusedMember.Global
        public abstract IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values);

        public abstract IEnumerator<TRow> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}