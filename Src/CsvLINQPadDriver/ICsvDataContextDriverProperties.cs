using System;
using System.Collections.Generic;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver
{
    public interface ICsvDataContextDriverProperties
    {
        bool IsProduction { get; set; }

        bool Persist { get; set; }

        /// <summary>
        /// Path to directory with CSV files or directly to CSV file.
        /// </summary>
        string Files { get; set; }

        /// <summary>
        /// Default file type.
        /// </summary>
        FileType FileType { get; set; }

        /// <summary>
        /// Files order by.
        /// </summary>
        FilesOrderBy FilesOrderBy { get; set; }

        /// <summary>
        /// Files without BOM encoding.
        /// </summary>
        NoBomEncoding NoBomEncoding { get; set; }

        /// <summary>
        /// Ignore malformed CSV data.
        /// </summary>
        bool IgnoreBadData { get; set; }

        /// <summary>
        /// Auto-detect encoding.
        /// </summary>
        bool AutoDetectEncoding { get; set; }

        /// <summary>
        /// Allow CSV comments.
        /// </summary>
        bool AllowComments { get; set; }

        /// <summary>
        /// Single-line comment characters.
        /// </summary>
        string CommentChars { get; set; }

        char? CommentChar { get; }

        IEnumerable<string> ParsedFiles { get; }

        /// <summary>
        /// Default CSV separator. If empty or null, separator will be auto-detected.
        /// </summary>
        string CsvSeparator { get; set; }

        string? SafeCsvSeparator { get; }

        /// <summary>
        /// Ignore blank lines.
        /// </summary>
        bool IgnoreBlankLines { get; }

        /// <summary>
        /// Fields whitespace trimming options.
        /// </summary>
        WhitespaceTrimOptions WhitespaceTrimOptions { get; }

        /// <summary>
        /// Use CsvHelper separator auto detection.
        /// </summary>
        bool UseCsvHelperSeparatorAutoDetection { get; }

        /// <summary>
        /// Create records instead of classes.
        /// </summary>
        bool UseRecordType { get; set; }

        /// <summary>
        /// If <c>true</c> - generates single class for similar CSV files.
        /// </summary>
        bool UseSingleClassForSameFiles { get; set; }

        /// <summary>
        /// If <c>true</c> - shows similar files non-grouped in addition to similar files groups.
        /// </summary>
        bool ShowSameFilesNonGrouped { get; set; }

        /// <summary>
        /// Generated class methods string comparison.
        /// </summary>
        StringComparison StringComparison { get; set; }

        /// <summary>
        /// If <c>true</c> - relations between CSV files/tables will be detected and created. (based on files and column names).
        /// </summary>
        bool DetectRelations { get; set; }

        /// <summary>
        /// If <c>true</c> - LINQPad will not show relations content in .Dump(). This prevents loading too many data.
        /// </summary>
        bool HideRelationsFromDump { get; set; }

        /// <summary>
        /// If <c>true</c> - some additional debug info is accessible.
        /// </summary>
        bool DebugInfo { get; set; }

        /// <summary>
        /// If <c>true</c> - check if file paths are valid.
        /// </summary>
        bool ValidateFilePaths { get; set; }

        /// <summary>
        /// Beginning of every file will be scanned and suspicious files with format not similar to CSV will be ignored
        /// </summary>
        bool IgnoreInvalidFiles { get; set; }

        /// <summary>
        /// Allow other processes to modify files being read.
        /// </summary>
        bool DoNotLockFiles { get; set; }

        /// <summary>
        /// If enabled, all string values are interned. 
        /// Can significantly reduce memory consumption, when values in CSV are repeated many times. 
        /// Custom per context interning is used.
        /// </summary>
        bool IsStringInternEnabled { get; set; }

        /// <summary>
        /// Compare interned strings using generation string comparer.
        /// </summary>
        bool UseStringComparerForStringIntern { get; set; }

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