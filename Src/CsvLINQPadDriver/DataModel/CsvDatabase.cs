using System.Collections.Generic;

namespace CsvLINQPadDriver.DataModel
{
    public record CsvDatabase(
        string Name,
        IList<CsvTable> Tables,
        IReadOnlyCollection<string> Files
    );
}
