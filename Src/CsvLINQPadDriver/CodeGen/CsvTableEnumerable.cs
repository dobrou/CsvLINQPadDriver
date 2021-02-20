using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvTableEnumerable<TRow> : CsvTableBase<TRow>
        where TRow : CsvRowBase, new()
    {
        public CsvTableEnumerable(bool isStringInternEnabled, char csvSeparator, string filePath, ICollection<CsvColumnInfo> propertiesInfo, Action<TRow> relationsInit)
            : base(isStringInternEnabled, csvSeparator, filePath, propertiesInfo, relationsInit)
        {
        }

        public override IEnumerator<TRow> GetEnumerator() =>
            ReadData().GetEnumerator();

        public override IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values) =>
            this.Where(r => values.Contains(getProperty(r), StringComparer));
    }
}
