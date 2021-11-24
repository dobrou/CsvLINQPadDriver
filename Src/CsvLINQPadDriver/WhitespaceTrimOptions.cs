using System.ComponentModel;

namespace CsvLINQPadDriver
{
    public enum WhitespaceTrimOptions
    {
        [Description("Around fields and inside quotes around fields")]
        All,

        [Description("Around fields")]
        Trim,

        [Description("Inside quotes around fields")]
        InsideQuotes
    }
}
