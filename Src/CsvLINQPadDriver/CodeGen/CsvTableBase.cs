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
        private readonly bool _allowComments;
        private readonly bool _ignoreBadData;

        protected readonly string FilePath;

        protected CsvTableBase(
            bool isStringInternEnabled,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            bool ignoreBadData,
            string filePath,
            IEnumerable<CsvColumnInfo> propertiesInfo,
            Action<TRow> relationsInit)
            : base(isStringInternEnabled)
        {
            _csvSeparator = csvSeparator;
            _noBomEncoding = noBomEncoding;
            _allowComments = allowComments;
            _ignoreBadData = ignoreBadData;

            FilePath = filePath;

            _cachedCsvRowMappingBase ??= new CsvRowMappingBase<TRow>(propertiesInfo, relationsInit);
        }

        protected IEnumerable<TRow> ReadData() =>
            FilePath.CsvReadRows(_csvSeparator, IsStringInternEnabled, _noBomEncoding, _allowComments, _ignoreBadData, _cachedCsvRowMappingBase!);

        // ReSharper disable once UnusedMember.Global
        public abstract IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values);

        public abstract IEnumerator<TRow> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}