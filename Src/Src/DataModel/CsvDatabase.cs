using System.Collections.Generic;

namespace CsvLINQPadDriver.DataModel
{
    public class CsvDatabase
    {
        public List<CsvTable> Tables { get; set; }
        public string Name { get; set; }

        public CsvDatabase()
        {
            Tables = new List<CsvTable>();
        }
    }
}
