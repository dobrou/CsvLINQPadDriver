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
            StringComparer? internStringComparer,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            WhitespaceTrimOptions whitespaceTrimOptions,
            string filePath,
            IEnumerable<CsvColumnInfo> propertiesInfo,
            Action<TRow> relationsInit)
            : base(
                isStringInternEnabled,
                internStringComparer,
                csvSeparator,
                noBomEncoding,
                allowComments,
                commentChar,
                ignoreBadData,
                autoDetectEncoding,
                ignoreBlankLines,
                whitespaceTrimOptions,
                filePath,
                propertiesInfo,
                relationsInit)
        {
        }

        public override IEnumerator<TRow> GetEnumerator() =>
            ReadData().GetEnumerator();

        public override IEnumerable<TRow> WhereIndexed(Func<TRow, string> getProperty, string propertyName, params string[] values) =>
            this.Where(row => values.Contains(getProperty(row), StringComparer));
    }
}
