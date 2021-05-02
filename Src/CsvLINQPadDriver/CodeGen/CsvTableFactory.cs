using System;
using System.Collections.Generic;

namespace CsvLINQPadDriver.CodeGen
{
    public static class CsvTableFactory
    {
        // ReSharper disable once UnusedMember.Global
        public static CsvTableBase<TRow> CreateTable<TRow>(
            bool isStringInternEnabled,
            bool isCacheEnabled,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            string filePath,
            IEnumerable<CsvColumnInfo> propertiesInfo,
            Action<TRow> relationsInit)
            where TRow : ICsvRowBase, new() =>
            isCacheEnabled
                // ReSharper disable once RedundantCast
                ? (CsvTableBase<TRow>)new CsvTableList<TRow>(isStringInternEnabled, csvSeparator, noBomEncoding, allowComments, filePath, propertiesInfo, relationsInit)
                : new CsvTableEnumerable<TRow>(isStringInternEnabled, csvSeparator, noBomEncoding, allowComments, filePath, propertiesInfo, relationsInit);
    }
}