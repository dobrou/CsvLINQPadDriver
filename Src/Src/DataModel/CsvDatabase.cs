using System.Collections.Generic;

namespace CsvLINQPadDriver.DataModel
{
    public class CsvDatabase
    {
        public IList<CsvTable> Tables { get; set; }
        public string Name { get; set; }

        public CsvDatabase()
        {
            Tables = new List<CsvTable>();
        }
    }
}
