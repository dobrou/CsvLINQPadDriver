using System.Diagnostics.CodeAnalysis;

namespace CsvLINQPadDriver.CodeGen
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface ICsvRowBase
    {
        string this[int index] { get; set; }
        string this[string index] { get; set; }
    }
}
