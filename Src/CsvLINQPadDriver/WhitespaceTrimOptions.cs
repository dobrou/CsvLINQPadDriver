using System.ComponentModel;

namespace CsvLINQPadDriver
{
    public enum WhitespaceTrimOptions
    {
        [Description("None")]
        None,

        [Description("Around fields")]
        Trim,

        [Description("Inside quotes around fields")]
        InsideQuotes
    }
}
