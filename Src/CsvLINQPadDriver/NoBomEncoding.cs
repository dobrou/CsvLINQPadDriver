using System.ComponentModel;

namespace CsvLINQPadDriver
{
    public enum NoBomEncoding
    {
        [Description("UTF-8")]
        UTF8,
        [Description("UTF-16")]
        Unicode,
        [Description("UTF-16 Big Endian")]
        BigEndianUnicode,
        [Description("UTF-32")]
        UTF32,
        [Description("UTF-32 Big Endian")]
        BigEndianUTF32,
        ASCII,
        [Description("System сode page")]
        SystemCodePage,
        [Description("User сode page")]
        UserCodePage
    }
}
