using System;
using System.Collections.Generic;

namespace CsvLINQPadDriver.CodeGen
{
    public static class CsvTableFactory
    {
        public static readonly bool IsCacheStatic = true;

        public static CsvTableBase<TRow> CreateTable<TRow>(
            bool isStringInternEnabled,
            bool isCacheEnabled,
            char csvSeparator,
            string filePath,
            ICollection<CsvColumnInfo> propertiesInfo,
            Action<TRow> relationsInit)
            where TRow : CsvRowBase, new() =>
            isCacheEnabled
                ? (CsvTableBase<TRow>)new CsvTableList<TRow>(isStringInternEnabled, csvSeparator, filePath, propertiesInfo, relationsInit)
                : new CsvTableEnumerable<TRow>(isStringInternEnabled, csvSeparator, filePath, propertiesInfo, relationsInit);
    }
}