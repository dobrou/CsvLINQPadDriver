using System.ComponentModel;

namespace CsvLINQPadDriver
{
    public enum HeaderDetection
    {
        [Description("Header is missing")]
        NoHeader,

        [Description("Header is present")]
        HasHeader,

        [Description("All letters｜Numbers｜Punctuation")]
        AllLettersNumbersPunctuation,

        [Description("All letters｜Numbers")]
        AllLettersNumbers,

        [Description("All letters only")]
        AllLetters,

        [Description("Latin letters｜Numbers｜Punctuation")]
        LatinLettersNumbersPunctuation,

        [Description("Latin letters｜Numbers")]
        LatinLettersNumbers,

        [Description("Latin letters only")]
        LatinLetters
    }
}
