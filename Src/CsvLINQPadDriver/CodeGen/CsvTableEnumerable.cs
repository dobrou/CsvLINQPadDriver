using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvLINQPadDriver.CodeGen
{
    internal class CsvTableEnumerable<TRow> : CsvTableBase<TRow>
        where TRow : ICsvRowBase, new()
    {
        public CsvTableEnumerable(
            bool isStringInternEnabled,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            bool ignoreBadData,
            string filePath,
            IEnumerable<CsvColumnInfo> propertiesInfo,
            Action<TRow> relationsInit)
            : base(isStringInternEnabled, csvSeparator, noBomEncoding, allowComments, ignoreBadData, filePath, propertiesInfo, relationsInit)
        {
        }

        public override IEnumerator<TRow> GetEnumerator() =>
            ReadData().GetEnumerator();

        public override IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values) =>
            this.Where(row => values.Contains(getProperty(row), StringComparer));
    }
}
