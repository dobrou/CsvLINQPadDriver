// ReSharper disable UnusedMember.Global

namespace CsvLINQPadDriver.CodeGen
{
    public interface ICsvRowBase
    {
        string this[int index] { get; set; }
        string this[string index] { get; set; }
    }
}
