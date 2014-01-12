using System.Collections.Generic;

namespace CsvLINQPadDriver.DataModel
{
    public class CsvTable
    {
        public string CodeName { get; set; }
        public string DisplayName { get; set; }

        public string FilePath { get; set; }
        public char CsvSeparator { get; set; }

        public IList<CsvColumn> Columns { get; set; }
        public IList<CsvRelation> Relations { get; set; } 

        public CsvTable()
        {
            Columns = new List<CsvColumn>();
            Relations = new List<CsvRelation>();
        }
    }
}
