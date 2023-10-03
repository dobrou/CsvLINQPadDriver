using System.ComponentModel;

namespace CsvLINQPadDriver;

public enum FilesOrderBy
{
    [Description("None")]
    None,

    [Description("Name (A to Z)")]
    NameAsc,

    [Description("Name (Z to A)")]
    NameDesc,

    [Description("Last write time (oldest to newest)")]
    LastWriteTimeAsc,

    [Description("Last write time (newest to oldest)")]
    LastWriteTimeDesc,

    [Description("Size (smallest to largest)")]
    SizeAsc,

    [Description("Size (largest to smallest)")]
    SizeDesc
}
