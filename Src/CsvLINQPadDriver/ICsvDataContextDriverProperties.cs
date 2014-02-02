namespace CsvLINQPadDriver
{
    public interface ICsvDataContextDriverProperties
    {
        bool Persist { get; set; }

        /// <summary>
        /// Path to directory with csv files or directly to csv file
        /// </summary>
        string Files { get; set; }

        /// <summary>
        /// Default csv separator. If empty or null, separator will be autodetected
        /// </summary>
        string CsvSeparator { get; set; }

        char? CsvSeparatorChar { get; }

        /// <summary>
        /// If True - relations between csv files/tables will be detected and created. (based on files and column names)
        /// </summary>
        bool DetectRelations { get; set; }

        /// <summary>
        /// If True - LINQPad will not show relations content in .Dump(). This prevents loading too many data.
        /// </summary>
        bool HideRelationsFromDump { get; set; }

        /// <summary>
        /// If True - some additional debug info is accessible
        /// </summary>
        bool DebugInfo { get; set; }

        /// <summary>
        /// Beginning of every file will be scanned and suspicious files with format not similar to CSV will be ingored
        /// </summary>
        bool IgnoreInvalidFiles { get; set; }

        /// <summary>
        /// True - Parsed rows from file are cached.
        /// This cache survives multiple query runs, even when query is changed.
        /// Chache is cleared as soon as LINQPad clears Application Domain of query.
        /// False - disable cache. Multiple enumerations of file content results in multiple reads and parsing of file.
        /// Can be significantly slower for complex queries.
        /// Significantly reduces memory usage. Useful when reading very large files.
        /// </summary>
        bool IsCacheEnabled { get; set; }
    }
}