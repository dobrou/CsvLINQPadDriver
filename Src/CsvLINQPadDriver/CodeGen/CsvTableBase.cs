using System;
using System.Collections;
using System.Collections.Generic;

using CsvLINQPadDriver.Helpers;

namespace CsvLINQPadDriver.CodeGen
{
    public class CsvTableBase
    {
        public static readonly StringComparer StringComparer = StringComparer.Ordinal;

        internal bool IsStringInternEnabled { get; }

        public CsvTableBase(bool isStringInternEnabled) =>
            IsStringInternEnabled = isStringInternEnabled;
    }

    public abstract class CsvTableBase<TRow> : CsvTableBase, IEnumerable<TRow>
        where TRow : CsvRowBase, new()
    {
        public char CsvSeparator { get; }
        public string FilePath { get; }

        internal ICollection<CsvColumnInfo> PropertiesInfo;
        internal Action<TRow> RelationsInit;

        protected CsvTableBase(bool isStringInternEnabled, char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
            : base(isStringInternEnabled)
        {
            CsvSeparator = csvSeparator;
            FilePath = filePath;
            PropertiesInfo = propertiesInfo;
            RelationsInit = relationsInit;
        }

        protected IEnumerable<TRow> ReadData() =>
            FileUtils.CsvReadRows(FilePath, CsvSeparator, IsStringInternEnabled, new CsvRowMappingBase<TRow>(PropertiesInfo, RelationsInit));

        public abstract IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values);

        public abstract IEnumerator<TRow> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}