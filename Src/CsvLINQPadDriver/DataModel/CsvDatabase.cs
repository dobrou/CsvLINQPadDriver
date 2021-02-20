using System.Collections.Generic;

namespace CsvLINQPadDriver.DataModel
{
    public class CsvDatabase
    {
        public IList<CsvTable> Tables { get; set; } = new List<CsvTable>();
        public string Name { get; set; }
    }
}
