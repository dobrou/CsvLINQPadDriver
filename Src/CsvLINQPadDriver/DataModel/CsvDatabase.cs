using System;
using System.Collections.Generic;

namespace CsvLINQPadDriver.DataModel
{
    internal record CsvDatabase(
        string Name,
        IList<CsvTable> Tables,
        IReadOnlyCollection<string> Files,
        IReadOnlyCollection<Exception> Exceptions
    );
}
