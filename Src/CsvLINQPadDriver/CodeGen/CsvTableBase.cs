﻿using System;
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
        private readonly bool _ignoreBadData;
        private readonly bool _autoDetectEncoding;

        protected readonly string FilePath;

        protected CsvTableBase(
            bool isStringInternEnabled,
            StringComparer? internStringComparer,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            bool ignoreBadData,
            bool autoDetectEncoding,
            string filePath,
            IEnumerable<CsvColumnInfo> propertiesInfo,
            Action<TRow> relationsInit)
            : base(isStringInternEnabled)
        {
            _internStringComparer = internStringComparer;
            _csvSeparator = csvSeparator;
            _noBomEncoding = noBomEncoding;
            _allowComments = allowComments;
            _ignoreBadData = ignoreBadData;
            _autoDetectEncoding = autoDetectEncoding;

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
                _ignoreBadData,
                _autoDetectEncoding,
                _cachedCsvRowMappingBase!);

        // ReSharper disable once UnusedMember.Global
        public abstract IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values);

        public abstract IEnumerator<TRow> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}