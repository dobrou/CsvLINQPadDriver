using System;
using System.Collections.Generic;

namespace CsvLINQPadDriver.CodeGen
{
    public static class CsvTableFactory
    {
        // ReSharper disable once UnusedMember.Global
        public static CsvTableBase<TRow> CreateTable<TRow>(
            bool isStringInternEnabled,
            StringComparer? internStringComparer,
            bool isCacheEnabled,
            char? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            bool ignoreBadData,
            bool autoDetectEncoding,
            string filePath,
            IEnumerable<CsvColumnInfo> propertiesInfo,
            Action<TRow> relationsInit)
            where TRow : ICsvRowBase, new() =>
            isCacheEnabled
                ? new CsvTableList<TRow>(
                    isStringInternEnabled,
                    internStringComparer,
                    csvSeparator,
                    noBomEncoding,
                    allowComments,
                    ignoreBadData,
                    autoDetectEncoding,
                    filePath,
                    propertiesInfo,
                    relationsInit)
                : new CsvTableEnumerable<TRow>(
                    isStringInternEnabled,
                    internStringComparer,
                    csvSeparator,
                    noBomEncoding,
                    allowComments,
                    ignoreBadData,
                    autoDetectEncoding,
                    filePath,
                    propertiesInfo,
                    relationsInit);
    }
}