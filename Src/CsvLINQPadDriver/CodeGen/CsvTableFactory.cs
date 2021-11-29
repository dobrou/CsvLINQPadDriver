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
            string? csvSeparator,
            NoBomEncoding noBomEncoding,
            bool allowComments,
            char? commentChar,
            bool ignoreBadData,
            bool autoDetectEncoding,
            bool ignoreBlankLines,
            bool doNotLockFiles,
            bool addHeader,
            HeaderDetection? headerDetection,
            WhitespaceTrimOptions? whitespaceTrimOptions,
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
                    commentChar,
                    ignoreBadData,
                    autoDetectEncoding,
                    ignoreBlankLines,
                    doNotLockFiles,
                    addHeader,
                    headerDetection,
                    whitespaceTrimOptions,
                    filePath,
                    propertiesInfo,
                    relationsInit)
                : new CsvTableEnumerable<TRow>(
                    isStringInternEnabled,
                    internStringComparer,
                    csvSeparator,
                    noBomEncoding,
                    allowComments,
                    commentChar,
                    ignoreBadData,
                    autoDetectEncoding,
                    ignoreBlankLines,
                    doNotLockFiles,
                    addHeader,
                    headerDetection,
                    whitespaceTrimOptions,
                    filePath,
                    propertiesInfo,
                    relationsInit);
    }
}