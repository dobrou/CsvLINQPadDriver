using System.Diagnostics.CodeAnalysis;

namespace CsvLINQPadDriver
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface ICsvDataContextDriverProperties
    {
        bool Persist { get; set; }

        /// <summary>
        /// Path to directory with CSV files or directly to CSV file.
        /// </summary>
        string Files { get; set; }

        public string[] ParsedFiles { get; }

        /// <summary>
        /// Default CSV separator. If empty or null, separator will be auto-detected.
        /// </summary>
        string CsvSeparator { get; set; }

        char? CsvSeparatorChar { get; }

        /// <summary>
        /// If <c>true</c> - relations between CSV files/tables will be detected and created. (based on files and column names)
        /// </summary>
        bool DetectRelations { get; set; }

        /// <summary>
        /// If <c>true</c> - LINQPad will not show relations content in .Dump(). This prevents loading too many data.
        /// </summary>
        bool HideRelationsFromDump { get; set; }

        /// <summary>
        /// If <c>true</c> - some additional debug info is accessible
        /// </summary>
        bool DebugInfo { get; set; }

        /// <summary>
        /// Beginning of every file will be scanned and suspicious files with format not similar to CSV will be ignored
        /// </summary>
        bool IgnoreInvalidFiles { get; set; }

        /// <summary>
        /// If enabled, all string values are interned. 
        /// Can significantly reduce memory consumption, when values in CSV are repeated many times. 
        /// Custom per context interning is used.
        /// </summary>
        bool IsStringInternEnabled { get; set; }

        /// <summary>
        /// <c>true</c> - Parsed rows from file are cached.
        /// This cache survives multiple query runs, even when query is changed.
        /// Cache is cleared as soon as LINQPad clears Application Domain of query.
        /// False - disable cache. Multiple enumerations of file content results in multiple reads and parsing of file.
        /// Can be significantly slower for complex queries.
        /// Significantly reduces memory usage. Useful when reading very large files.
        /// </summary>
        bool IsCacheEnabled { get; set; }
    }
}